using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Services;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Constants;
using MyNotes.Models.Media;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Windows;
using MyNotes.Strings;
using MyNotes.ViewModels.Media.Providers;

using Windows.System;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageViewModel : ViewModelBase
{
  private readonly NoteImageService NoteImageService;
  private readonly ImageCollectionViewModelProvider ImageCollectionViewModelProvider;
  private readonly ImageViewerWindowService ImageViewerWindowService;
  private readonly DialogService DialogService;

  public ImageDescriptor ImageDescriptor { get; }

  public ImageCollectionKey CollectionKey => ImageDescriptor.CollectionKey;

  private readonly FileSystemWatcher _imageFileWatcher;

  private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

  private Lazy<BitmapImage> _imageLazy;

  public BitmapImage? Image
  {
    get => field ??= _imageLazy.Value;
    private set => SetProperty(ref field, value);
  }

  #region Object Lifetime Management
  public ImageViewModel(NoteImageService noteImageService, ImageCollectionViewModelProvider imageCollectionViewModelProvider, ImageViewerWindowService imageViewerWindowService, DialogService dialogService, ImageDescriptor imageDescriptor)
  {
    NoteImageService = noteImageService;
    ImageCollectionViewModelProvider = imageCollectionViewModelProvider;
    ImageViewerWindowService = imageViewerWindowService;
    DialogService = dialogService;
    ImageDescriptor = imageDescriptor;

    ResetImageLazy();

    _imageFileWatcher = new(ImageDescriptor.LocalImageFolderPath)
    {
      Filter = ImageDescriptor.LocalImageFileName,
      NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
      EnableRaisingEvents = true
    };
    _imageFileWatcher.Changed += ImageFileWatcher_Changed;
    _imageFileWatcher.Deleted += ImageFileWatcher_Deleted;
    SetCommands();
  }

  private readonly Lock _syncRoot = new();

  [MemberNotNull(nameof(_imageLazy))]
  private void ResetImageLazy()
  {
    lock (_syncRoot)
    {
      Image = null;
      _imageLazy = new Lazy<BitmapImage>(() => new BitmapImage
      {
        UriSource = new Uri(ImageDescriptor.LocalImageFilePath),
        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
      }, LazyThreadSafetyMode.ExecutionAndPublication);
    }
  }

  private void ImageFileWatcher_Changed(object sender, FileSystemEventArgs e)
  {
    _dispatcherQueue.TryEnqueue(() =>
    {
      ResetImageLazy();
      ImageChanged?.Invoke(this, new ImageChangedEventArgs(ImageChangeKind.Modified));
    });
  }

  private void ImageFileWatcher_Deleted(object sender, FileSystemEventArgs e)
  {
    _dispatcherQueue.TryEnqueue(() =>
    {
      ResetImageLazy();
      ImageChanged?.Invoke(this, new ImageChangedEventArgs(ImageChangeKind.Deleted));
    });
  }

  public event EventHandler<ImageChangedEventArgs>? ImageChanged;

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      _imageFileWatcher.Dispose();
      _imageFileWatcher.Changed -= ImageFileWatcher_Changed;
      _imageFileWatcher.Deleted -= ImageFileWatcher_Deleted;
    }

    base.Dispose(disposing);
  }
  #endregion
}

internal sealed partial class ImageViewModel : ViewModelBase
{
  public AsyncCommand OpenInFileExplorerCommand { get; private set; }
  public AsyncCommand OpenInPhotosCommand { get; private set; }

  public AsyncCommand OpenWithCommand { get; private set; }
  public AsyncCommand ShowImageCommand { get; private set; }
  public AsyncCommand<Microsoft.UI.WindowId> SaveImageCommand { get; private set; }
  public AsyncCommand<XamlRoot> DeleteImageCommand { get; private set; }

  [MemberNotNull(nameof(OpenInFileExplorerCommand), nameof(OpenInPhotosCommand), nameof(OpenWithCommand), nameof(ShowImageCommand), nameof(SaveImageCommand), nameof(DeleteImageCommand))]
  private void SetCommands()
  {
    OpenInFileExplorerCommand = new()
    {
      ExecuteFunc = async () =>
      {
        try
        {
          StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(ImageDescriptor.LocalImageFolderPath);
          FolderLauncherOptions options = new();
          options.ItemsToSelect.Add(await StorageFile.GetFileFromPathAsync(ImageDescriptor.LocalImageFilePath));

          await Launcher.LaunchFolderAsync(localFolder, options);
        }
        catch
        { }
      }
    };

    OpenInPhotosCommand = new()
    {
      ExecuteFunc = async () =>
      {
        try
        {
          Uri photosUri = AppStrings.GetPhotosUri(ImageDescriptor.LocalImageFilePath);
          await Launcher.LaunchUriAsync(photosUri, new LauncherOptions()
          {
            TreatAsUntrusted = false
          });
        }
        catch
        { }
      }
    };

    OpenWithCommand = new()
    {
      ExecuteFunc = async () =>
      {
        try
        {
          StorageFile localFile = await StorageFile.GetFileFromPathAsync(ImageDescriptor.LocalImageFilePath);
          await Launcher.LaunchFileAsync(localFile, new LauncherOptions()
          {
            DisplayApplicationPicker = true,
            TreatAsUntrusted = false
          });
        }
        catch
        { }
      }
    };

    ShowImageCommand = new()
    {
      ExecuteFunc = async () =>
      {
        var imageViewerWindow = await ImageViewerWindowService.GetOrCreate(CollectionKey, this.ImageDescriptor);
        imageViewerWindow.Activate();
      }
    };

    SaveImageCommand = new()
    {
      ExecuteFunc = async (windowId) =>
      {
        FileSavePicker savePicker = new(windowId)
        {
          SuggestedFileName = ImageDescriptor.OriginalFileName,
          DefaultFileExtension = ImageDescriptor.StoredExtension,
          ShowOverwritePrompt = true,
          SuggestedStartLocation = PickerLocationId.Desktop
        };

        savePicker.FileTypeChoices.Add(LocalizedStrings.FileSavePickerOriginalFileFormat, [ImageDescriptor.StoredExtension]);

        PickFileResult pickFileResult = await savePicker.PickSaveFileAsync();
        if (pickFileResult is not null)
        {
          if (await NoteImageService.GetImageAsync(ImageDescriptor.Id) is NoteImageDto imageDto)
          {
            try
            {
              StorageFolder destinationFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(pickFileResult.Path));
              StorageFile localFile = await StorageFile.GetFileFromPathAsync(ImageDescriptor.LocalImageFilePath);
              await localFile.CopyAsync(destinationFolder, Path.GetFileName(pickFileResult.Path), NameCollisionOption.ReplaceExisting);
            }
            catch { }
          }
        }
      }
    };

    DeleteImageCommand = new()
    {
      ExecuteFunc = async (xamlRoot) =>
      {
        var dialogResponse = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, "Image", "Image", DeleteMode.MoveToTrash);
        if (dialogResponse.Result == ContentDialogResult.Primary)
        {
          switch (dialogResponse.Data)
          {
            case DeleteMode.MoveToTrash:
              {
                await NoteImageService.DeleteImageAsync(ImageDescriptor.Id);
                await using var lease = await ImageCollectionViewModelProvider.AcquireAsync(CollectionKey);
                lease?.ViewModel.RemoveImageFromCollection(this);
              }
              break;
            case DeleteMode.Permanent:
              break;
          }
        }
      }
    };
  }
}

public class ImageChangedEventArgs(ImageChangeKind imageChangeKind) : EventArgs
{
  public ImageChangeKind Kind { get; } = imageChangeKind;
}

public enum ImageChangeKind
{
  Modified,
  Replaced,
  Deleted
}