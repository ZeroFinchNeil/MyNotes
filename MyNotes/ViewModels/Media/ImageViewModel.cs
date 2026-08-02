using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Media.Services;
using MyNotes.Common.Commands;
using MyNotes.Models.Media;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageViewModel : ViewModelBase
{
  private readonly ImageService ImageService;

  public ImageDescriptor ImageDescriptor { get; }
  public BitmapImage? Image { get; }

  public bool Failed { get; private set; } = false;

  #region Object Lifetime Management
  public ImageViewModel(ImageService imageService, ImageDescriptor imageDescriptor)
  {
    ImageService = imageService;
    ImageDescriptor = imageDescriptor;

    Image = new()
    {
      UriSource = new Uri(ImageDescriptor.FilePath),
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
  public AsyncCommand<Microsoft.UI.WindowId> SaveImageCommand { get; private set; }

  [MemberNotNull(nameof(SaveImageCommand))]
  private void SetCommands()
  {
    SaveImageCommand = new()
    {
      ExecuteFunc = async (windowId) =>
      {
        FolderPicker picker = new(windowId);
        PickFolderResult pickFolderResult = await picker.PickSingleFolderAsync();
        if (pickFolderResult is not null)
        {
          if (await ImageService.GetImageAsync(ImageDescriptor.Id) is ImageDto imageDto)
          {
            var folder = await StorageFolder.GetFolderFromPathAsync(pickFolderResult.Path);
            StorageFile originalFile = await StorageFile.GetFileFromPathAsync(ImageDescriptor.FilePath);
            await originalFile.CopyAsync(folder, $"{imageDto.OriginalFileName}{imageDto.StoredExtension}", NameCollisionOption.GenerateUniqueName);
          }
        }
      }
    };
  }
}