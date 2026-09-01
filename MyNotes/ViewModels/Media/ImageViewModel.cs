using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Services;
using MyNotes.Common.Commands;
using MyNotes.Models.Media;
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

  public ImageDescriptor ImageDescriptor { get; }

  public ImageCollectionKey CollectionKey => ImageDescriptor.CollectionKey;

  private readonly FileSystemWatcher _imageFileWatcher;

  private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

  private Lazy<BitmapImage> _imageLazy;

  public BitmapImage Image
  {
    get => _imageLazy.Value;
    set => OnPropertyChanged();
  }

  public bool Failed { get; private set; } = false;

  #region Object Lifetime Management
  public ImageViewModel(NoteImageService noteImageService, ImageCollectionViewModelProvider imageCollectionViewModelProvider, ImageViewerWindowService imageViewerWindowService, ImageDescriptor imageDescriptor)
  {
    NoteImageService = noteImageService;
    ImageCollectionViewModelProvider = imageCollectionViewModelProvider;
    ImageViewerWindowService = imageViewerWindowService;
    ImageDescriptor = imageDescriptor;

    CreateImageLazy();

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
  private void CreateImageLazy()
  {
    lock (_syncRoot)
    {
      _imageLazy = new Lazy<BitmapImage>(() => new BitmapImage
      {
        UriSource = new Uri(ImageDescriptor.LocalImageFilePath),
        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
      });
    }
  }

  public void ResetImageCache() => CreateImageLazy();

  private void ImageFileWatcher_Changed(object sender, FileSystemEventArgs e)
  {
    //_dispatcherQueue.TryEnqueue(LoadImage);
  }

  private void ImageFileWatcher_Deleted(object sender, FileSystemEventArgs e)
  {
    //_dispatcherQueue.TryEnqueue(SetFallbackImage);
  }

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
  public AsyncCommand OpenWithCommand { get; private set; }
  public AsyncCommand ShowImageCommand { get; private set; }
  public AsyncCommand<Microsoft.UI.WindowId> SaveImageCommand { get; private set; }
  public AsyncCommand DeleteImageCommand { get; private set; }

  [MemberNotNull(nameof(OpenWithCommand), nameof(ShowImageCommand), nameof(SaveImageCommand), nameof(DeleteImageCommand))]
  private void SetCommands()
  {
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
      ExecuteFunc = async () =>
      {
        if (!Failed && File.Exists(ImageDescriptor.LocalImageFilePath))
        {
          await NoteImageService.DeleteImageAsync(ImageDescriptor.Id);
          using var lease = ImageCollectionViewModelProvider.Acquire(CollectionKey);
          lease?.ViewModel.RemoveImageFromCollection(this);
        }
      }
    };
  }
}