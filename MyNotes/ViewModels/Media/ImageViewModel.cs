using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Media.Services;
using MyNotes.Common.Commands;
using MyNotes.Models.Media;
using MyNotes.Strings;
using MyNotes.ViewModels.Media.Providers;

using Windows.System;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageViewModel : ViewModelBase
{
  private readonly ImageService ImageService;
  private readonly ImageCollectionViewModelProvider ImageCollectionViewModelProvider;

  public ImageDescriptor ImageDescriptor { get; }

  public BitmapImage? Image { get; }

  public bool Failed { get; private set; } = false;

  #region Object Lifetime Management
  public ImageViewModel(ImageService imageService, ImageCollectionViewModelProvider imageCollectionViewModelProvider, ImageDescriptor imageDescriptor)
  {
    ImageService = imageService;
    ImageCollectionViewModelProvider = imageCollectionViewModelProvider;
    ImageDescriptor = imageDescriptor;

    Image = new()
    {
      UriSource = new Uri(ImageDescriptor.LocalFilePath),
      DecodePixelType = DecodePixelType.Logical,
      DecodePixelHeight = 1024
    };
    Image.ImageFailed += Image_ImageFailed;

    SetCommands();
  }
  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      Image?.ImageFailed -= Image_ImageFailed;
    }

    base.Dispose(disposing);
  }
  #endregion

  private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
  {
    Image?.UriSource = new Uri("ms-appx:///Assets/Images/Failure.png");
    Failed = true;
  }

  public async Task<bool> DeleteImageAsync()
  {
    if (!Failed)
    {
      await ImageService.DeleteImageAsync(ImageDescriptor.Id);
      return true;
    }

    return false;
  }
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
        StorageFile localFile = await StorageFile.GetFileFromPathAsync(ImageDescriptor.LocalFilePath);
        await Launcher.LaunchFileAsync(localFile, new LauncherOptions()
        {
          DisplayApplicationPicker = true,
          TreatAsUntrusted = false
        });
      }
    };

    ShowImageCommand = new()
    {
      ExecuteFunc = async () =>
      {
        using var lease = ImageCollectionViewModelProvider.Acquire(ImageDescriptor.ParentKey);
        if (lease is not null)
        {
          await lease.ViewModel.ShowImageCommand.ExecuteAsync(this);
        }
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
          ShowOverwritePrompt = true
        };

        savePicker.FileTypeChoices.Add(LocalizedStrings.FileSavePickerOriginalFileFormat, [ImageDescriptor.StoredExtension]);

        PickFileResult pickFileResult = await savePicker.PickSaveFileAsync();
        if (pickFileResult is not null)
        {
          if (await ImageService.GetImageAsync(ImageDescriptor.Id) is ImageDto imageDto)
          {
            StorageFolder destinationFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(pickFileResult.Path));
            StorageFile localFile = await StorageFile.GetFileFromPathAsync(ImageDescriptor.LocalFilePath);
            await localFile.CopyAsync(destinationFolder, Path.GetFileName(pickFileResult.Path), NameCollisionOption.ReplaceExisting);
          }
        }
      }
    };

    DeleteImageCommand = new()
    {
      ExecuteFunc = async () =>
      {
        using var lease = ImageCollectionViewModelProvider.Acquire(ImageDescriptor.ParentKey);
        if (lease is not null)
        {
          await lease.ViewModel.DeleteImageCommand.ExecuteAsync(this);
        }
      }
    };
  }
}