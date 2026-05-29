using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Windows.Storage.Pickers;

using MyNotes.Common.Commands;
using MyNotes.Models.Media;
using MyNotes.Services.App;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.Shared.Constants;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageCollectionViewModel : ViewModelBase
{
  private readonly ImageViewModelProvider ImageViewModelProvider;
  private readonly WindowService WindowService;

  private ImageCollectionKey ImageCollectionKey { get; }

  #region Object Lifetime Management
  public ImageCollectionViewModel(ImageViewModelProvider imageViewModelProvider, WindowService windowService, ImageCollectionKey imageCollectionKey)
  {
    ImageViewModelProvider = imageViewModelProvider;
    WindowService = windowService;

    ImageCollectionKey = imageCollectionKey;

    ImageViewModels = ImageCollectionKey.CollectionReference.TryGetTarget(out var collection) ? collection : new();
    SetCommands();
  }
  #endregion

  [ObservableProperty]
  public partial ObservableCollection<ImageViewModel> ImageViewModels { get; private set; }

  [ObservableProperty]
  public partial ImageViewModel? SelectedImage { get; set; }
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
      ExecuteAction = async (noteId) =>
      {
        if (WindowService.TryGetNoteWindowInfo(noteId, out _, out var appWindow))
        {
          FileOpenPicker picker = new(appWindow.OwnerWindowId)
          {
            ViewMode = PickerViewMode.Thumbnail
          };

          foreach (var fileType in AppStrings.BitmapImageFileTypeFilter)
          {
            picker.FileTypeFilter.Add(fileType);
          }

          foreach (var result in await picker.PickMultipleFilesAsync())
          {
            try
            {
              // 원본 이미지 파일 가져오기
              var originalPath = result.Path;
              var originalFile = await StorageFile.GetFileFromPathAsync(originalPath);

              // LocalFolder의 Image 폴더 안에 이미지 파일 복사
              ImageDescriptor imageDescriptor = ImageDescriptor.Create(originalPath);

              var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AppStrings.ImageFolderName, CreationCollisionOption.OpenIfExists);
              var copiedFile = await originalFile.CopyAsync(folder, imageDescriptor.FileName, NameCollisionOption.ReplaceExisting);

              if (ImageViewModelProvider.Resolve(imageDescriptor) is ImageViewModel imageViewModel)
              {
                ImageViewModels.Add(imageViewModel);
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
      ExecuteAction = async (imageViewModel) =>
      {
        var imageViewerWindow = await WindowService.GetOrCreateImageViewerWindow(ImageCollectionKey);
        imageViewerWindow.Activate();
        if (ImageViewModels.Contains(imageViewModel))
        {
          SelectedImage = imageViewModel;
        }
      }
    };

    DeleteImageCommand = new()
    {
      ExecuteAction = async (imageViewModel) =>
      {
        if (await imageViewModel.DeleteImageAsync())
        {
          ImageViewModels.Remove(imageViewModel);
        }
      }
    };
  }
}
