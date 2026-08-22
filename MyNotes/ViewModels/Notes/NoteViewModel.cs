using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Application.Contracts.Converters;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Commands;
using MyNotes.Application.Notes.Services;
using MyNotes.Application.Results;
using MyNotes.Application.Settings.Services;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Updates;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase, IAsyncDisposable
{
  private readonly NoteCommandService NoteCommandService;
  private readonly NavigationCommandService NavigationCommandService;
  private readonly NoteService NoteService;
  private readonly IUpdateCoordinator<string, NoteViewStatePatchDto> NoteUpdateCoordinator;
  private readonly AppSettingsService AppSettingsService;

  public NoteModel Note { get; }

  #region Object Lifetime Management
  public NoteViewModel([FromKeyedServices(CommandServiceType.Note)] ICommandService noteCommandService, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, NoteService noteService, IUpdateCoordinator<string, NoteViewStatePatchDto> updateCoordinator, AppSettingsService appSettingsService, NoteModel note)
  {
    // DI
    NoteCommandService = (NoteCommandService)noteCommandService;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;
    NoteService = noteService;
    NoteUpdateCoordinator = updateCoordinator;
    AppSettingsService = appSettingsService;

    Note = note;

    SetBackgroundImage();
    Note.PropertyChanged += Note_PropertyChanged;
    SetCommands();
  }

  private bool _disposeStarted;
  private async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    Note.PropertyChanged -= Note_PropertyChanged;
    await NoteService.Modification.CommitSearchIndexAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    Dispose(disposing: false);
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
      NoteUpdateCoordinator.Submit(persistenceDescriptor.Key, persistenceDescriptor.CreatePatch(Note), persistenceDescriptor.BatchMode);
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

  public async Task<bool> DeleteNotePermanentlyWhenEmpty()
  {
    //if (AppSettingsService.Load(AppSettingsDescriptors.DeleteEmptyNote) && string.IsNullOrEmpty(Note.Title) && string.IsNullOrWhiteSpace(RtfTextConverter.ToPlainText(Note.Body)))
    //{
    //  DeleteNoteAppCommand appCommand = new()
    //  {
    //    Id = Note.Id,
    //    DeleteMode = DeleteMode.Permanent
    //  };
    //  return await NoteService.Modification.DeleteNoteAsync(appCommand) is AppUpdateStatus.Succeeded;
    //}

    //return false;
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
  private void SetCommands()
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