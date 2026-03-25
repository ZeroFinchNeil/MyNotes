using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.AppConstants;
using MyNotes.Common.Commands;
using MyNotes.Models.Media;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageViewModel : ViewModelBase
{
  public ImageDescriptor ImageDescriptor { get; }
  public BitmapImage? Image { get; }

  public bool Failed { get; private set; } = false;

  #region Object Lifetime Management
  public ImageViewModel(ImageDescriptor imageDescriptor)
  {
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
      return;

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
    try
    {
      if (!Failed && await ApplicationData.Current.LocalFolder.CreateFolderAsync(AppStrings.ImageFolderPath, CreationCollisionOption.OpenIfExists) is StorageFolder folder)
      {
        var file = await folder.GetFileAsync(ImageDescriptor.FileName);
        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
      }
      return true;
    }
    catch (FileNotFoundException)
    {
      return true;
    }
    catch
    {
      return false;
    }
  }
}

internal sealed partial class ImageViewModel : ViewModelBase
{
  public Command? SaveImageCommand { get; private set; }

  private void SetCommands()
  {
    SaveImageCommand = new()
    {
      ActionToExecute = () =>
      {
      }
    };
  }
}