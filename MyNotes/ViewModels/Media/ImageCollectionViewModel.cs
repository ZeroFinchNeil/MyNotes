using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Windows.Storage.Pickers;

using MyNotes.Application.Media.Commands;
using MyNotes.Application.Media.Services;
using MyNotes.Common.Commands;
using MyNotes.Common.Mappers;
using MyNotes.Constants;
using MyNotes.Domain.Notes;
using MyNotes.Services.Windows;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.ViewModels.Media;

internal sealed partial class ImageCollectionViewModel : ViewModelBase
{
  private readonly ImageService ImageService;
  private readonly ImageViewModelProvider ImageViewModelProvider;
  private readonly NoteWindowService NoteWindowService;
  private readonly ImageViewerWindowService ImageViewerWindowService;

  private NoteId NoteId { get; }

  #region Object Lifetime Management
  public ImageCollectionViewModel(ImageService imageService, ImageViewModelProvider imageViewModelProvider, NoteWindowService noteWindowService, ImageViewerWindowService imageViewerWindowService, NoteId noteId)
  {
    ImageService = imageService;
    ImageViewModelProvider = imageViewModelProvider;
    NoteWindowService = noteWindowService;
    ImageViewerWindowService = imageViewerWindowService;

    NoteId = noteId;

    _ = InitializeAsync();
    SetCommands();
  }
  #endregion

  private async Task InitializeAsync()
  {
    var imageDtos = await ImageService.GetImagesAsync(NoteId);
    ImageViewModels = [.. imageDtos.Select(i => ImageViewModelProvider.Resolve(ImageMapper.ToModel(i)))];
  }

  [ObservableProperty]
  public partial ObservableCollection<ImageViewModel> ImageViewModels { get; private set; } = [];

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
          // FilePicker 열기
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
              var imageDto = await ImageService.AttachImageAsync(new AttachImageAppCommand()
              {
                NoteId = NoteId,
                OriginalFilePath = pickFileResult.Path
              });

              var imageDescriptor = ImageMapper.ToModel(imageDto);

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
