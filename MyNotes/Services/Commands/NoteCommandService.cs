using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Commands.Notes;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Results;
using MyNotes.Application.Services.App;
using MyNotes.Application.Services.Notes;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Helpers;
using MyNotes.Common.Interop;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;
using MyNotes.Models;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.Shared.Constants;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Services.Commands;

internal sealed class NoteCommandService : ICommandService
{
  private readonly NoteService NoteService;
  private readonly NoteWindowService NoteWindowService;
  private readonly IModelFactory<NoteDto, NoteModel> NoteModelFactory;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly NoteListViewModelProvider NoteListViewModelProvider;
  private readonly NavigationController NavigationController;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly MainWindowService MainWindowService;
  private readonly DialogService DialogService;
  private readonly JumpListService JumpListService;
  private readonly SettingsService SettingsService;

  public Command<NoteModel> OpenNoteWindowCommand { get; }
  public Command<NoteModel> MinimizeNoteWindowCommand { get; }
  public Command<NoteModel> CloseNoteWindowCommand { get; }
  public Command<SourceTargetPair<NoteModel, NavigationId>> MoveNoteToListCommand { get; }
  public AsyncCommand<NavigationId?> CreateNewNoteCommand { get; }
  public Command<NoteModel> ViewListCommand { get; }
  public AsyncCommand<NoteModel> ToggleBookmarkNoteCommand { get; }
  public AsyncCommand<NoteModel> RemoveNoteCommand { get; }
  public Command<NoteModel> AddNoteToJumpListCommand { get; }

  public NoteCommandService(
    NoteService noteService,
    NoteWindowService noteWindowService,
    IModelFactory<NoteDto, NoteModel> noteModelFactory,
    NoteViewModelProvider noteViewModelProvider,
    NoteListViewModelProvider noteListViewModelProvider,
    NavigationController navigationController,
    NavigationViewModelProvider navigationViewModelProvider,
    MainWindowService mainWindowService,
    DialogService dialogService,
    JumpListService jumpListService,
    SettingsService settingsService
    )
  {
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    NoteModelFactory = noteModelFactory;
    NoteViewModelProvider = noteViewModelProvider;
    NoteListViewModelProvider = noteListViewModelProvider;
    NavigationController = navigationController;
    NavigationViewModelProvider = navigationViewModelProvider;
    MainWindowService = mainWindowService;
    DialogService = dialogService;
    JumpListService = jumpListService;
    SettingsService = settingsService;

    OpenNoteWindowCommand = new()
    {
      ExecuteAction = async (noteModel) => await NoteWindowService.OpenNoteWindow(noteModel)
    };

    MinimizeNoteWindowCommand = new()
    {
      ExecuteAction = async (note) =>
      {
        if (NoteWindowService.TryGetWindowInfo(note.Id, out _, out var appWindow))
        {
          var presenter = appWindow?.Presenter as OverlappedPresenter;
          presenter?.Minimize();
        }
      }
    };

    CloseNoteWindowCommand = new()
    {
      ExecuteAction = async (noteModel) =>
      {
        if (NoteWindowService.TryGetWindowInfo(noteModel.Id, out IntPtr hWnd, out _))
        {
          NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
        }
      }
    };

    MoveNoteToListCommand = new()
    {
      ExecuteAction = async (pair) =>
      {
        NoteModel sourceNote = pair.Source;
        NavigationId oldNavigationId = sourceNote.NavigationId;
        NavigationId newNavigationId = pair.Target;

        if (sourceNote.NavigationId == newNavigationId)
        {
          return;
        }

        sourceNote.NavigationId = newNavigationId;

        UpdateNoteAppCommand updateAppCommand = new()
        {
          PatchDto = new NotePatchDto()
          {
            Id = sourceNote.Id,
            NavigationId = newNavigationId,
          }
        };

        var updateResult = await NoteService.Modification.UpdateNoteAsync(updateAppCommand);

        if (updateResult.Status is AppUpdateStatus.Succeeded)
        {
          if (NavigationViewModelProvider.TryResolve(oldNavigationId, out var s)
              && s is UserListNavigationViewModel sourceViewModel
              && NoteListViewModelProvider.TryResolve(sourceViewModel.Navigation, out var noteListViewModel)
              && noteListViewModel.NoteViewModels?.FirstOrDefault(vm => vm.Note == sourceNote) is NoteViewModel sourceNoteViewModel)
          {
            noteListViewModel.NoteViewModels.Remove(sourceNoteViewModel);
          }
          sourceNote.Modified = updateResult.Modified ?? throw new InvalidOperationException();
        }
      }
    };

    CreateNewNoteCommand = new()
    {
      ExecuteFunc = async (navigationId) =>
      {
        if (navigationId is NavigationId targetNavigationId
            && NavigationViewModelProvider.TryResolve(targetNavigationId, out var nvm)
            && nvm is UserListNavigationViewModel navigationViewModel)
        {
          var size = SettingsService.Load(AppSettingsDescriptors.NoteSize).SizeInt32;
          var position = MainWindowService.GetNewWindowPosition(size) ?? AppDefaultSettings.WindowPosition.PointInt32;

          CreateNoteAppCommand appCommand = new()
          {
            NavigationId = navigationViewModel.Navigation.Id,
            Size = size,
            Position = position
          };

          if (await NoteService.Creation.AddNoteAsync(appCommand) is NoteDto newNoteDto)
          {
            NoteModel newNoteModel = NoteModelFactory.Create(newNoteDto);
            NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNoteModel);
            await NoteWindowService.OpenNoteWindow(newNoteModel);

            if (NoteListViewModelProvider.TryResolve(navigationViewModel.Navigation, out var noteListViewModel)
                && noteListViewModel.NoteViewModels is NoteViewModelCollection noteViewModels
                && !noteViewModels.Contains(newNoteViewModel))
            {
              noteViewModels.Add(newNoteViewModel);
            }
          }
        }
        else
        {
          //todo: JumpList에 의해 생성되는 등 Parent Navigation이 정해지지 않았을 때 노트 생성 로직 구현

          //NoteId newNoteId = await NoteService.GetUniqueNoteIdAsync();
          //NoteModel newNoteModel = NoteModelStore.Resolve(newNoteId, () => NoteModelFactory.CreateDefault(newNoteId));
          //NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNoteModel);
          //await NoteWindowService.OpenNoteWindow(newNoteModel);
        }
      }
    };

    ViewListCommand = new()
    {
      ExecuteAction = async (noteModel) =>
      {
        var mainWindow = await MainWindowService.GetOrCreate(noteModel.NavigationId);
        mainWindow.Activate();
      }
    };

    ToggleBookmarkNoteCommand = new()
    {
      ExecuteFunc = async (noteModel) =>
      {
        var oldState = noteModel.IsBookmarked;
        var newState = !oldState;
        UpdateNoteAppCommand appCommand = new()
        {
          PatchDto = new NotePatchDto()
          {
            Id = noteModel.Id,
            IsBookmarked = new(newState)
          }
        };
        var updateResult = await NoteService.Modification.UpdateNoteAsync(appCommand);
        if (updateResult.Status is AppUpdateStatus.Succeeded)
        {
          noteModel.IsBookmarked = newState;
          WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(noteModel, nameof(NoteModel.IsBookmarked), oldState, newState), AppMessageTokens.ChangeNoteIsBookmarkedStateToken);
          noteModel.Modified = updateResult.Modified ?? throw new InvalidOperationException();
        }
      }
    };

    RemoveNoteCommand = new()
    {
      ExecuteFunc = async (noteModel) =>
      {
        if (MainWindowService.TryGetCurrentWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var preferredDeleteMode = DeleteMode.MoveToTrash;
          var dialogResponse = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, "Note", noteModel.Title, preferredDeleteMode);
          if (dialogResponse.Result == ContentDialogResult.Primary)
          {
            DeleteNoteAppCommand deleteCommand = new()
            {
              Id = noteModel.Id,
              DeleteMode = dialogResponse.Data
            };

            var deleteResult = await NoteService.Modification.DeleteNoteAsync(deleteCommand);
            if (deleteResult is AppUpdateStatus.Succeeded)
            {
              noteModel.IsDeleted = true;

              if (NavigationController.CurrentNavigation is INavigationNoteList navigation
                  && NoteViewModelProvider.TryResolve(noteModel, out var noteViewModel))
              {
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<NoteViewModel>(noteViewModel), AppMessageTokens.RemoveNoteFromListToken(navigation));
              }
            }
          }
        }
      }
    };

    AddNoteToJumpListCommand = new()
    {
      ExecuteAction = async (noteModel) => await JumpListService.AddToJumpListAsync(NoteMappers.ToDomain(noteModel))
    };
  }

  public async Task RenameNoteTitle(NoteModel noteModel, string oldTitle)
  {
    string newTitle = noteModel.Title;
    UpdateNoteAppCommand appCommand = new()
    {
      PatchDto = new NotePatchDto()
      {
        Id = noteModel.Id,
        Title = new(newTitle)
      }
    };

    var updateResult = await NoteService.Modification.UpdateNoteAsync(appCommand);

    if (updateResult.Status is AppUpdateStatus.Succeeded)
    {
      noteModel.Modified = updateResult.Modified ?? throw new InvalidOperationException();
      await JumpListService.EditJumpListItemAsync(NoteMappers.ToDomain(noteModel));
      return;
    }

    noteModel.Title = oldTitle;
  }
}
