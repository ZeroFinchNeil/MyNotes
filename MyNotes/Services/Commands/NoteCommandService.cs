using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Notes.Commands;
using MyNotes.Application.Notes.Services;
using MyNotes.Application.Results;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Helpers;
using MyNotes.Common.Interop;
using MyNotes.Common.Mappers;
using MyNotes.Constants;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;
using MyNotes.Models;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Settings;
using MyNotes.Services.Shell;
using MyNotes.Services.Windows;
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
  private readonly ViewStateSettingsService ViewStateSettingsService;

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
    ViewStateSettingsService viewStateSettingsService
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
    ViewStateSettingsService = viewStateSettingsService;
  }

  public Task OpenNoteWindowAsync(NoteModel noteModel) => NoteWindowService.OpenNoteWindow(noteModel);

  public void MinimizeNoteWindow(NoteId noteId)
  {
    if (NoteWindowService.TryGetWindowInfo(noteId, out _, out var appWindow))
    {
      var presenter = appWindow?.Presenter as OverlappedPresenter;
      presenter?.Minimize();
    }
  }

  public void CloseNoteWindow(NoteId noteId)
  {
    if (NoteWindowService.TryGetWindowInfo(noteId, out IntPtr hWnd, out _))
    {
      NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
    }
  }

  public void PinNoteWindow(NoteId noteId, bool isPinned)
  {
    if (NoteWindowService.TryGetWindowInfo(noteId, out _, out var appWindow)
      && appWindow?.Presenter is OverlappedPresenter presenter)
    {
      presenter.IsAlwaysOnTop = isPinned;
    }
  }

  public async Task MoveNoteToListAsync(NoteModel sourceNoteModel, NavigationId targetNavigationId)
  {
    NavigationId oldNavigationId = sourceNoteModel.NavigationId;

    if (sourceNoteModel.NavigationId == targetNavigationId)
    {
      return;
    }

    sourceNoteModel.NavigationId = targetNavigationId;

    UpdateNoteAppCommand updateAppCommand = new()
    {
      PatchDto = new NotePatchDto()
      {
        Id = sourceNoteModel.Id,
        NavigationId = targetNavigationId,
      }
    };

    var updateResult = await NoteService.Modification.UpdateNoteAsync(updateAppCommand);

    if (updateResult.Status is AppUpdateStatus.Succeeded)
    {
      if (NavigationViewModelProvider.TryResolve(oldNavigationId, out var s)
          && s is UserListNavigationViewModel sourceViewModel
          && NoteListViewModelProvider.TryResolve(sourceViewModel.Navigation, out var noteListViewModel)
          && noteListViewModel.NoteViewModels?.FirstOrDefault(vm => vm.Note == sourceNoteModel) is NoteViewModel sourceNoteViewModel)
      {
        noteListViewModel.NoteViewModels.Remove(sourceNoteViewModel);
      }
      sourceNoteModel.Modified = updateResult.Modified ?? throw new InvalidOperationException();
    }
  }

  public async Task CreateNewNoteAsync(NavigationId? navigationId = null)
  {
    if (navigationId is NavigationId targetNavigationId
      && NavigationViewModelProvider.TryResolve(targetNavigationId, out var nvm)
      && nvm is UserListNavigationViewModel navigationViewModel)
    {
      var size = ViewStateSettingsService.Load<SizeInt32, Size>(s => new((int)s.Width, (int)s.Height), ViewStateSettingsDescriptors.NoteSize);
      var position = MainWindowService.GetNewWindowPosition(size) ?? ViewStateSettingsDescriptors.NoteWindowPosition.PointInt32;

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

  public async Task RenameNoteTitleAsync(NoteModel noteModel, string oldTitle)
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

  public async Task ToggleBookmarkNoteAsync(NoteModel noteModel)
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

  public async Task RemoveNoteAsync(NoteModel noteModel)
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

  public Task AddNoteToJumpList(NoteModel noteModel) => JumpListService.AddToJumpListAsync(NoteMappers.ToDomain(noteModel));
}
