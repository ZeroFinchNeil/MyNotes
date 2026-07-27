using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Windows.Storage.Pickers;

using MyNotes.Common.Commands;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Media;
using MyNotes.Services.Windows;
using MyNotes.Shared.Constants;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageCollectionViewModel : ViewModelBase
{
  private readonly ImageViewModelProvider ImageViewModelProvider;
  private readonly NoteWindowService NoteWindowService;
  private readonly ImageViewerWindowService ImageViewerWindowService;

  private NoteId NoteId { get; }

  #region Object Lifetime Management
  public ImageCollectionViewModel(ImageViewModelProvider imageViewModelProvider, NoteWindowService noteWindowService, ImageViewerWindowService imageViewerWindowService, NoteId noteId)
  {
    ImageViewModelProvider = imageViewModelProvider;
    NoteWindowService = noteWindowService;
    ImageViewerWindowService = imageViewerWindowService;

    NoteId = noteId;

    ImageViewModels = NoteId.CollectionReference.TryGetTarget(out var collection) ? collection : new();
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
  public Command InsertImageCommand { get; private set; }

  public Command<ImageViewModel> ShowImageCommand { get; private set; }
  public Command<ImageViewModel> DeleteImageCommand { get; private set; }

  [MemberNotNull(nameof(InsertImageCommand), nameof(ShowImageCommand), nameof(DeleteImageCommand))]
  private void SetCommands()
  {
    InsertImageCommand = new()
    {
      ExecuteAction = async () =>
      {
        if (NoteWindowService.TryGetWindowInfo(NoteId, out _, out var appWindow))
        {
          FileOpenPicker picker = new(appWindow.OwnerWindowId)
          {
            ViewMode = PickerViewMode.Thumbnail
          };

          foreach (var fileType in AppStrings.BitmapImageFileTypeFilter)
          {
            picker.FileTypeFilter.Add(fileType);
          }

          foreach (var pickFileResult in await picker.PickMultipleFilesAsync())
          {
            try
            {
              // 원본 이미지 파일 가져오기
              var originalPath = pickFileResult.Path;
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
        var imageViewerWindow = await ImageViewerWindowService.GetOrCreate(NoteId);
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
