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

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
    }

    base.Dispose(disposing);
  }
  #endregion

  private async Task InitializeAsync()
  {
    var imageDtos = await ImageService.GetImagesByNoteIdAsync(NoteId);
    ImageViewModels = [.. imageDtos.Select(i => ImageViewModelProvider.Resolve(ImageMapper.ToModel(i)))];
  }

  [ObservableProperty]
  public partial ObservableCollection<ImageViewModel> ImageViewModels { get; private set; } = [];

  [ObservableProperty]
  public partial ImageViewModel? SelectedImage { get; set; }

  public async Task<bool> MoveImageAsync(int sourceIndex, int targetIndex)
  {
    if (sourceIndex < 0 || sourceIndex >= ImageViewModels.Count || targetIndex < 0 || targetIndex >= ImageViewModels.Count)
    {
      return false;
    }

    await ImageService.MoveImageAsync(new MoveImageAppCommand()
    {
      SourceId = ImageViewModels[sourceIndex].ImageDescriptor.Id,
      TargetId = ImageViewModels[targetIndex].ImageDescriptor.Id
    });

    return true;
  }
}

internal sealed partial class ImageCollectionViewModel : ViewModelBase
{
  public AsyncCommand<Microsoft.UI.WindowId> InsertImageCommand { get; private set; }

  public Command<ImageViewModel> ShowImageCommand { get; private set; }
  public Command<ImageViewModel> DeleteImageCommand { get; private set; }

  [MemberNotNull(nameof(InsertImageCommand), nameof(ShowImageCommand), nameof(DeleteImageCommand))]
  private void SetCommands()
  {
    InsertImageCommand = new()
    {
      ExecuteFunc = async (windowId) =>
      {
        // FilePicker 열기
        FileOpenPicker picker = new(windowId)
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
