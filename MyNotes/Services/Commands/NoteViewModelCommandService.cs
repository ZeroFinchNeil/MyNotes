using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.AppConstants;
using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Modes;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Notes;
using MyNotes.Services.Windows;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Services.Commands;

internal sealed class NoteViewModelCommandService : ICommandService
{
  private readonly NoteService NoteService;
  private readonly NavigationService NavigationService;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly NoteListViewModelProvider NoteListViewModelProvider;
  private readonly WindowService WindowService;
  private readonly DialogService DialogService;

  public Command<NoteViewModel> OpenWindowCommand { get; }
  public Command<SourceTargetPair<NoteViewModel, NavigationId>> MoveToListCommand { get; }
  public Command<NoteViewModel> CreateNewNoteCommand { get; }
  public Command<NoteViewModel> ViewListCommand { get; }
  public Command<NoteViewModel> RemoveNoteCommand { get; }

  public NoteViewModelCommandService(NoteService noteService, NavigationService navigationService, NavigationViewModelProvider navigationViewModelProvider, NoteViewModelProvider noteViewModelProvider, NoteListViewModelProvider noteListViewModelProvider, WindowService windowService, DialogService dialogService)
  {
    NoteService = noteService;
    NavigationService = navigationService;
    NavigationViewModelProvider = navigationViewModelProvider;
    NoteViewModelProvider = noteViewModelProvider;
    NoteListViewModelProvider = noteListViewModelProvider;
    WindowService = windowService;
    DialogService = dialogService;

    OpenWindowCommand = new()
    {
      ActionToExecute = (noteViewModel) =>
      {
        NoteService.OpenNoteWindow(noteViewModel.Note);
      }
    };

    MoveToListCommand = new()
    {
      ActionToExecute = async (pair) =>
      {
        NoteViewModel sourceNoteViewModel = pair.Source;
        Note sourceNote = sourceNoteViewModel.Note;
        NavigationId oldNavigationId = sourceNote.NavigationId;
        NavigationId newNavigationId = pair.Target;

        if (sourceNote.NavigationId == newNavigationId)
          return;

        sourceNote.NavigationId = newNavigationId;
        await NoteService.UpdateNoteEntityAsync(sourceNote, entity => entity.Parent = newNavigationId.Value);

        if (NavigationViewModelProvider.TryResolve(oldNavigationId, out var s)
            && s is UserLeafNavigationViewModel sourceViewModel
            && NoteListViewModelProvider.TryResolve(sourceViewModel.Navigation, out var noteListViewModel))
        {
          noteListViewModel?.NoteViewModels?.Remove(sourceNoteViewModel);
        }
      }
    };

    CreateNewNoteCommand = new()
    {
      ActionToExecute = async (noteViewModel) =>
      {
        if (NavigationViewModelProvider.TryResolve(noteViewModel.Note.NavigationId, out var nvm)
            && nvm is UserLeafNavigationViewModel navigationViewModel)
        {
          if (await NoteService.AddNoteAsync(navigationViewModel.Navigation) is Note newNote)
          {
            NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNote);
            NoteService.OpenNoteWindow(newNote);

            if (NoteListViewModelProvider.TryResolve(navigationViewModel.Navigation, out var noteListViewModel))
            {
              noteListViewModel.NoteViewModels?.Add(newNoteViewModel);
            }
          }
        }
      }
    };

    ViewListCommand = new()
    {
      ActionToExecute = (noteViewModel) =>
      {
        var mainWindow = WindowService.GetOrCreateMainWindow(noteViewModel.Note.NavigationId);
        mainWindow.Activate();
      }
    };

    RemoveNoteCommand = new()
    {
      ActionToExecute = async (noteViewModel) =>
      {
        if (WindowService.TryGetCurrentMainWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var deleteMode = DeleteMode.MoveToTrash;
          var result = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, "Note", noteViewModel.Note.Title, deleteMode);
          if (result.ContentDialogResult == ContentDialogResult.Primary)
          {
            switch (result.DeleteMode)
            {
              case DeleteMode.MoveToTrash:
                break;
              case DeleteMode.Permanent:
                await NoteService.DeleteNotePermanentlyAsync(noteViewModel.Note.Id);
                break;
            }
          }
          noteViewModel.Note.IsDeleted = true;

          if (NavigationService.CurrentNavigation is INavigationNoteList navigation)
          {
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<NoteViewModel>(noteViewModel), AppMessageTokens.RemoveNoteFromListToken(navigation));
          }
        }
      }
    };
  }
}
