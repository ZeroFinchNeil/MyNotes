using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.AppConstants;
using MyNotes.Common.Commands;

namespace MyNotes.ViewModels.Images;

internal sealed partial class ImageViewModel : ViewModelBase
{
  public string FileName { get; }
  public BitmapImage? Image { get; }

  public bool LoadSucceeded { get; }

  public ImageViewModel(string fileName)
  {
    FileName = fileName;
    try
    {
      Image = new() { UriSource = new Uri(System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, AppStrings.ImageFolderPath, fileName)) };
      LoadSucceeded = true;
      SetCommands();
    }
    catch
    {
      LoadSucceeded = false;
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