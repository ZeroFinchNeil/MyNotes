using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Application.Media.Services;
using MyNotes.Common.Commands;
using MyNotes.Constants;
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
    if(!Failed)
    {
      await ImageService.DeleteImageAsync(ImageDescriptor.Id);
      return true;
    }

    return false;
  }
}

internal sealed partial class ImageViewModel : ViewModelBase
{
  public Command? SaveImageCommand { get; private set; }

  private void SetCommands()
  {
    SaveImageCommand = new()
    {
      ExecuteAction = () =>
      {
      }
    };
  }
}