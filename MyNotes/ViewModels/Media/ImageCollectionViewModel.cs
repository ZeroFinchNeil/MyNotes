using System.Security.Cryptography;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Windows.Storage.Pickers;

using MyNotes.AppConstants;
using MyNotes.Common.Commands;
using MyNotes.Models.Media;
using MyNotes.Models.Notes;
using MyNotes.Services.App;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageCollectionViewModel : ViewModelBase
{
  private readonly ImageViewModelProvider ImageViewModelProvider;
  private readonly WindowService WindowService;

  #region Object Lifetime Management
  public ImageCollectionViewModel(ImageViewModelProvider imageViewModelProvider, WindowService windowService)
  {
    ImageViewModelProvider = imageViewModelProvider;
    WindowService = windowService;

    SetCommands();
  }
  #endregion

  [ObservableProperty]
  public partial ObservableCollection<ImageViewModel>? ImageViewModels { get; private set; }

  [ObservableProperty]
  public partial ImageViewModel? SelectedImage { get; set; }

  public void SetImages(IEnumerable<ImageDescriptor> descriptors)
  {
    ImageViewModels = new();
    foreach (var descriptor in descriptors)
    {
      if (ImageViewModelProvider.Resolve(descriptor) is ImageViewModel imageViewModel)
      {
        Console.WriteLine("{0}: {1}", "imageViewModel", imageViewModel.ImageDescriptor.FileName);
        ImageViewModels.Add(imageViewModel);
      }
    }
  }

  public void SetImages(ObservableCollection<ImageViewModel> imageViewModels)
  {
    ImageViewModels = imageViewModels;
  }

  public void NavigateImage(ImageViewModel selection)
  {
    if (ImageViewModels is not null && ImageViewModels.Contains(selection))
    {
      SelectedImage = selection;
    }
  }
}

internal sealed partial class ImageCollectionViewModel : ViewModelBase
{
  public Command<NoteId>? InsertImageCommand { get; private set; }

  public Command<ImageViewModel>? ShowImageCommand { get; private set; }
  public Command<ImageViewModel>? DeleteImageCommand { get; private set; }

  private void SetCommands()
  {
    InsertImageCommand = new()
    {
      ActionToExecute = async (noteId) =>
      {
        if (WindowService.TryGetNoteWindowInfo(noteId, out _, out var appWindow))
        {
          FileOpenPicker picker = new(appWindow.OwnerWindowId)
          {
            ViewMode = PickerViewMode.Thumbnail
          };
          picker.FileTypeFilter.Add(".jpg");
          picker.FileTypeFilter.Add(".jpeg");
          picker.FileTypeFilter.Add(".png");
          picker.FileTypeFilter.Add(".bmp");
          picker.FileTypeFilter.Add(".gif");
          picker.FileTypeFilter.Add(".tiff");
          picker.FileTypeFilter.Add(".ico");

          foreach (var result in await picker.PickMultipleFilesAsync())
          {
            try
            {
              var originalPath = result.Path;
              var originalFile = await StorageFile.GetFileFromPathAsync(originalPath);
              byte[] randomBytes = new byte[16];
              RandomNumberGenerator.Fill(randomBytes);
              var fileName = System.IO.Path.ChangeExtension(Convert.ToHexStringLower(randomBytes), System.IO.Path.GetExtension(result.Path));

              var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AppStrings.ImageFolderPath, CreationCollisionOption.OpenIfExists);
              var copiedFile = await originalFile.CopyAsync(folder, fileName, NameCollisionOption.ReplaceExisting);
              ImageDescriptor imageDescriptor = new() { FileName = System.IO.Path.GetFileName(copiedFile.Path) };
              if (ImageViewModelProvider.Resolve(imageDescriptor) is ImageViewModel imageViewModel)
              {
                ImageViewModels?.Add(imageViewModel);
              }
            }
            catch (Exception e)
            {
              Console.WriteLine("{0}: {1}", "File Exception", e.Message);
            }
          }
        }
      }
    };

    ShowImageCommand = new()
    {
      ActionToExecute = async (imageViewModel) =>
      {
        if (ImageViewModels is not null)
        {
          var imageViewerWindow = await WindowService.GetOrCreateImageViewerWindow(ImageViewModels.Select(vm => vm.ImageDescriptor), imageViewModel.ImageDescriptor);
          imageViewerWindow.Activate();
        }
      }
    };

    DeleteImageCommand = new()
    {
      ActionToExecute = async (imageViewModel) =>
      {
        if (await imageViewModel.DeleteImageAsync())
        {
          ImageViewModels?.Remove(imageViewModel);
        }
      }
    };
  }
}
