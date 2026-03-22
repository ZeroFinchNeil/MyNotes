using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.AppConstants;
using MyNotes.Common.Commands;
using MyNotes.Common.Interop;
using MyNotes.Common.Structures;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.App;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Notes;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Services.Commands;

internal sealed class NoteCommandService : ICommandService
{
  private readonly NoteService NoteService;
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly NoteListViewModelProvider NoteListViewModelProvider;
  private readonly WindowService WindowService;
  private readonly DialogService DialogService;
  private readonly JumpListService JumpListService;

  public Command<Note> OpenNoteWindowCommand { get; }
  public Command<Note> MinimizeNoteWindowCommand { get; }
  public Command<Note> CloseNoteWindowCommand { get; }
  public Command<SourceTargetPair<Note, NavigationId>> MoveNoteToListCommand { get; }
  public Command<NavigationId?> CreateNewNoteCommand { get; }
  public Command<Note> ViewListCommand { get; }
  public Command<Note> RemoveNoteCommand { get; }
  public Command<Note> AddNoteToJumpListCommand { get; }

  public NoteCommandService(NoteService noteService, NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, NoteViewModelProvider noteViewModelProvider, NoteListViewModelProvider noteListViewModelProvider, WindowService windowService, DialogService dialogService, JumpListService jumpListService)
  {
    NoteService = noteService;
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NoteViewModelProvider = noteViewModelProvider;
    NoteListViewModelProvider = noteListViewModelProvider;
    WindowService = windowService;
    DialogService = dialogService;
    JumpListService = jumpListService;

    OpenNoteWindowCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        await NoteService.OpenNoteWindow(note);
      }
    };

    MinimizeNoteWindowCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        if (WindowService.TryGetNoteWindowInfo(note.Id, out _, out var appWindow))
        {
          var presenter = appWindow?.Presenter as OverlappedPresenter;
          presenter?.Minimize();
        }
      }
    };

    CloseNoteWindowCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        if (WindowService.TryGetNoteWindowInfo(note.Id, out IntPtr hWnd, out _))
        {
          NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
        }
      }
    };

    MoveNoteToListCommand = new()
    {
      ActionToExecute = async (pair) =>
      {
        Note sourceNote = pair.Source;
        NavigationId oldNavigationId = sourceNote.NavigationId;
        NavigationId newNavigationId = pair.Target;

        if (sourceNote.NavigationId == newNavigationId)
          return;

        sourceNote.NavigationId = newNavigationId;
        await NoteService.UpdateNoteEntityAsync(sourceNote, entity => entity.Parent = newNavigationId.Value);

        if (NavigationViewModelProvider.TryResolve(oldNavigationId, out var s)
            && s is UserLeafNavigationViewModel sourceViewModel
            && NoteListViewModelProvider.TryResolve(sourceViewModel.Navigation, out var noteListViewModel)
            && noteListViewModel.NoteViewModels?.FirstOrDefault(vm => vm.Note == sourceNote) is NoteViewModel sourceNoteViewModel)
        {
          noteListViewModel.NoteViewModels.Remove(sourceNoteViewModel);
        }
      }
    };

    CreateNewNoteCommand = new()
    {
      ActionToExecute = async (id) =>
      {
        if (id is NavigationId navigationId
            && NavigationViewModelProvider.TryResolve(navigationId, out var nvm)
            && nvm is UserLeafNavigationViewModel navigationViewModel)
        {
          if (await NoteService.AddNoteAsync(navigationViewModel.Navigation) is Note newNote)
          {
            NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNote);
            await NoteService.OpenNoteWindow(newNote);

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
          Note newNote = await NoteService.AddNoteAsync(null);
          NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNote);
          await NoteService.OpenNoteWindow(newNote);
        }
      }
    };

    ViewListCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        var mainWindow = await WindowService.GetOrCreateMainWindow(note.NavigationId);
        mainWindow.Activate();
      }
    };

    RemoveNoteCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        if (WindowService.TryGetCurrentMainWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var deleteMode = DeleteMode.MoveToTrash;
          var result = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, "Note", note.Title, deleteMode);
          if (result.ContentDialogResult == ContentDialogResult.Primary)
          {
            switch (result.DeleteMode)
            {
              case DeleteMode.MoveToTrash:
                break;
              case DeleteMode.Permanent:
                await NoteService.DeleteNotePermanentlyAsync(note.Id);
                break;
            }

            note.IsDeleted = true;

            if (NavigationService.CurrentNavigation is INavigationNoteList navigation
                && NoteViewModelProvider.TryResolve(note, out var noteViewModel))
            {
              WeakReferenceMessenger.Default.Send(new ValueChangedMessage<NoteViewModel>(noteViewModel), AppMessageTokens.RemoveNoteFromListToken(navigation));
            }
          }
        }
      }
    };

    AddNoteToJumpListCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        await JumpListService.AddToJumpListAsync(note);
      }
    };
  }
}
