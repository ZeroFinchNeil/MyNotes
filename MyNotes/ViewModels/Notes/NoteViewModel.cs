using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Application.Contracts.Converters;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Commands;
using MyNotes.Application.Notes.Services;
using MyNotes.Application.Results;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;
using MyNotes.Services.Updates;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private readonly NoteCommandService NoteCommandService;
  private readonly NavigationCommandService NavigationCommandService;
  private readonly NoteService NoteService;
  private readonly IUpdateCoordinator<string, NoteViewStatePatchDto> UpdateBatchCoordinator;
  private readonly ViewStateSettingsService ViewStateSettingsService;
  private readonly IRtfTextConverter RtfTextConverter;

  public NoteModel Note { get; }

  #region Object Lifetime Management
  public NoteViewModel([FromKeyedServices(CommandServiceType.Note)] ICommandService noteCommandService, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, NoteService noteService, IUpdateCoordinator<string, NoteViewStatePatchDto> updateBatchCoordinator, ViewStateSettingsService viewStateSettingsService, IRtfTextConverter rtfTextConverter, NoteModel note)
  {
    // DI
    NoteCommandService = (NoteCommandService)noteCommandService;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;
    NoteService = noteService;
    UpdateBatchCoordinator = updateBatchCoordinator;
    ViewStateSettingsService = viewStateSettingsService;
    RtfTextConverter = rtfTextConverter;

    Note = note;

    SetBackgroundImage();
    Note.Preview = RtfTextConverter.GetPreview(Note.Body, 0, 500);
    Note.PropertyChanged += Note_PropertyChanged;
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
      _ = NoteService.Modification.CommitSearchIndexAsync();
    }

    base.Dispose(disposing);
  }
  #endregion

  #region Note 내부 속성 변경 시 데이터베이스에 반영 및 기타 로직 실행

  private static readonly IReadOnlyDictionary<string, PatchDescriptor<NoteModel, string, NoteViewStatePatchDto>> ViewStatePatchDescriptors = new Dictionary<string, PatchDescriptor<NoteModel, string, NoteViewStatePatchDto>>()
  {
    [nameof(NoteModel.ShowBackgroundImage)] = new()
    {
      Key = nameof(NoteModel.ShowBackgroundImage),
      BatchMode = UpdateBatchMode.Unbatched,
      CreatePatch = (noteModel) => new NoteViewStatePatchDto()
      {
        Id = noteModel.Id,
        ShowBackgroundImage = noteModel.ShowBackgroundImage
      }
    },
  };

  private async void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is null)
    {
      return;
    }

    if (ViewStatePatchDescriptors.TryGetValue(e.PropertyName, out var persistenceDescriptor))
    {
      UpdateBatchCoordinator.Submit(persistenceDescriptor.Key, persistenceDescriptor.CreatePatch(Note), persistenceDescriptor.BatchMode);
    }

    // 뷰에 반영(TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.ShowBackgroundImage):
        if (Note.ShowBackgroundImage)
        {
          Note.BackdropKind = BackdropKind.None;
          SetBackgroundImage();
        }
        else
        {
          BackgroundImage = null;
        }
        break;
      case nameof(Note.BackgroundImagePath):
        SetBackgroundImage();
        break;
    }
  }

  public async Task UpdateNoteTitle(string oldTitle)
  {
    UpdateNoteAppCommand appCommand = new()
    {
      PatchDto = new NotePatchDto()
      {
        Id = Note.Id,
        Title = new(Note.Title)
      }
    };
    var updateResult = await NoteService.Modification.UpdateNoteAsync(appCommand);
  }

  public async Task<bool> DeleteNotePermanentlyWhenEmpty()
  {
    if (ViewStateSettingsService.Load(ViewStateSettingsDescriptors.DeleteEmptyNote) && string.IsNullOrEmpty(Note.Title) && string.IsNullOrWhiteSpace(RtfTextConverter.ToPlainText(Note.Body)))
    {
      DeleteNoteAppCommand appCommand = new()
      {
        Id = Note.Id,
        DeleteMode = DeleteMode.Permanent
      };
      return await NoteService.Modification.DeleteNoteAsync(appCommand) is AppUpdateStatus.Succeeded;
    }

    return false;
  }
  #endregion

  #region Background Image

  [ObservableProperty]
  public partial BitmapImage? BackgroundImage { get; set; }

  private static BitmapImage? GetBackgroundImage(string? imagePath)
  {
    if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
    {
      return null;
    }

    try
    {
      BitmapImage image = new()
      {
        UriSource = new Uri(imagePath),
        DecodePixelType = DecodePixelType.Logical
      };
      return image;
    }
    catch (Exception)
    { }

    return null;
  }

  private void SetBackgroundImage() => BackgroundImage = Note.ShowBackgroundImage ? GetBackgroundImage(Note.BackgroundImagePath) : null;
  #endregion

  #region Body Images
  [ObservableProperty]
  public partial bool IsImagePanelVisible { get; set; }

  [ObservableProperty]
  public partial double ImagePanelMaxHeight { get; set; } = 120.0;
  #endregion
}

partial class NoteViewModel
{
  public AsyncCommand OpenWindowCommand { get; private set; }
  public Command MinimizeWindowCommand { get; private set; }
  public Command CloseWindowCommand { get; private set; }
  public Command PinWindowCommand { get; private set; }
  public AsyncCommand<NavigationId> MoveToListCommand { get; private set; }
  public AsyncCommand CreateNewNoteCommand { get; private set; }
  public AsyncCommand ViewListCommand { get; private set; }
  public AsyncCommand RenameNoteTitleCommand { get; private set; }
  public AsyncCommand ToggleBookmarkNoteCommand { get; private set; }
  public AsyncCommand RemoveNoteCommand { get; private set; }
  public AsyncCommand AddNoteToJumpListCommand { get; private set; }

  public string OldTitle { get; set; } = string.Empty;

  [MemberNotNull(nameof(OpenWindowCommand), nameof(MinimizeWindowCommand), nameof(CloseWindowCommand), nameof(PinWindowCommand), nameof(MoveToListCommand), nameof(CreateNewNoteCommand), nameof(ViewListCommand), nameof(RenameNoteTitleCommand), nameof(ToggleBookmarkNoteCommand), nameof(RemoveNoteCommand), nameof(AddNoteToJumpListCommand))]
  public void SetCommands()
  {
    OpenWindowCommand = new()
    {
      ExecuteFunc = () => NoteCommandService.OpenNoteWindowAsync(Note)
    };

    MinimizeWindowCommand = new()
    {
      ExecuteAction = () => NoteCommandService.MinimizeNoteWindow(Note.Id)
    };

    CloseWindowCommand = new()
    {
      ExecuteAction = () => NoteCommandService.CloseNoteWindow(Note.Id)
    };

    PinWindowCommand = new()
    {
      ExecuteAction = () => NoteCommandService.PinNoteWindow(Note.Id, Note.IsAlwaysOnTop)
    };

    MoveToListCommand = new()
    {
      ExecuteFunc = (targetNavigationId) => NoteCommandService.MoveNoteToListAsync(Note, targetNavigationId)
    };

    CreateNewNoteCommand = new()
    {
      ExecuteFunc = () => NoteCommandService.CreateNewNoteAsync(Note.NavigationId)
    };

    ViewListCommand = new()
    {
      ExecuteFunc = () => NavigationCommandService.ViewNavigationListPageAsync(Note.NavigationId)
    };

    RenameNoteTitleCommand = new()
    {
      ExecuteFunc = () => NoteCommandService.RenameNoteTitleAsync(Note, OldTitle)
    };

    ToggleBookmarkNoteCommand = new()
    {
      ExecuteFunc = () => NoteCommandService.ToggleBookmarkNoteAsync(Note)
    };

    RemoveNoteCommand = new()
    {
      ExecuteFunc = () => NoteCommandService.RemoveNoteAsync(Note)
    };

    AddNoteToJumpListCommand = new()
    {
      ExecuteFunc = () => NoteCommandService.AddNoteToJumpList(Note)
    };
  }
}