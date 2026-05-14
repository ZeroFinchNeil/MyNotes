using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Dtos.Notes;
using MyNotes.Application.Services.App;
using MyNotes.Application.Services.Navigations;
using MyNotes.Application.Services.Notes;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Interop;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
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
  private readonly NoteModelFactory NoteModelFactory;
  private readonly NoteModelProvider NoteModelProvider;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly NoteListViewModelProvider NoteListViewModelProvider;
  private readonly NavigationController NavigationController;
  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly MainWindowService MainWindowService;
  private readonly DialogService DialogService;
  private readonly JumpListService JumpListService;

  public Command<NoteModel> OpenNoteWindowCommand { get; }
  public Command<NoteModel> MinimizeNoteWindowCommand { get; }
  public Command<NoteModel> CloseNoteWindowCommand { get; }
  public Command<SourceTargetPair<NoteModel, NavigationId>> MoveNoteToListCommand { get; }
  public Command<NavigationId?> CreateNewNoteCommand { get; }
  public Command<NoteModel> ViewListCommand { get; }
  public Command<NoteModel> RemoveNoteCommand { get; }
  public Command<NoteModel> AddNoteToJumpListCommand { get; }

  public NoteCommandService(
    NoteService noteService,
    NoteWindowService noteWindowService,
    NoteModelFactory noteModelFactory,
    NoteModelProvider noteModelProvider,
    NoteViewModelProvider noteViewModelProvider,
    NoteListViewModelProvider noteListViewModelProvider,
    NavigationController navigationController,
    NavigationViewModelProvider navigationViewModelProvider,
    MainWindowService mainWindowService,
    DialogService dialogService,
    JumpListService jumpListService
    )
  {
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    NoteModelFactory = noteModelFactory;
    NoteModelProvider = noteModelProvider;
    NoteViewModelProvider = noteViewModelProvider;
    NoteListViewModelProvider = noteListViewModelProvider;
    NavigationController = navigationController;
    NavigationViewModelProvider = navigationViewModelProvider;
    MainWindowService = mainWindowService;
    DialogService = dialogService;
    JumpListService = jumpListService;

    OpenNoteWindowCommand = new()
    {
      ActionToExecute = async (noteModel) => await NoteWindowService.OpenNoteWindow(noteModel)
    };

    MinimizeNoteWindowCommand = new()
    {
      ActionToExecute = async (note) =>
      {
        if (NoteWindowService.TryGetNoteWindowInfo(note.Id, out _, out var appWindow))
        {
          var presenter = appWindow?.Presenter as OverlappedPresenter;
          presenter?.Minimize();
        }
      }
    };

    CloseNoteWindowCommand = new()
    {
      ActionToExecute = async (noteModel) =>
      {
        if (NoteWindowService.TryGetNoteWindowInfo(noteModel.Id, out IntPtr hWnd, out _))
        {
          NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
        }
      }
    };

    MoveNoteToListCommand = new()
    {
      ActionToExecute = async (pair) =>
      {
        NoteModel sourceNote = pair.Source;
        NavigationId oldNavigationId = sourceNote.NavigationId;
        NavigationId newNavigationId = pair.Target;

        if (sourceNote.NavigationId == newNavigationId)
        {
          return;
        }

        sourceNote.NavigationId = newNavigationId;
        UpdateNoteAppRequestDto updateNoteDto = new()
        {
          Id = sourceNote.Id,
          NoteUpdateField = NoteUpdateFields.NavigationId,
          NavigationId = newNavigationId
        };

        await NoteService.Modification.UpdateNoteAsync(updateNoteDto);

        if (NavigationViewModelProvider.TryResolve(oldNavigationId, out var s)
            && s is UserListNavigationViewModel sourceViewModel
            && NoteListViewModelProvider.TryResolve(sourceViewModel.Navigation, out var noteListViewModel)
            && noteListViewModel.NoteViewModels?.FirstOrDefault(vm => vm.Note == sourceNote) is NoteViewModel sourceNoteViewModel)
        {
          noteListViewModel.NoteViewModels.Remove(sourceNoteViewModel);
        }
      }
    };

    CreateNewNoteCommand = new()
    {
      ActionToExecute = async (navigationId) =>
      {
        if (navigationId is NavigationId targetNavigationId
            && NavigationViewModelProvider.TryResolve(targetNavigationId, out var nvm)
            && nvm is UserListNavigationViewModel navigationViewModel)
        {
          if (await NoteService.Creation.AddNoteAsync(navigationViewModel.Navigation.Id) is NoteAppResponseDto newNoteDto)
          {
            NoteModel newNoteModel = NoteModelProvider.Resolve(newNoteDto.Id, () => NoteModelFactory.Create(newNoteDto));
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
          NoteId newNoteId = await NoteService.GetUniqueNoteIdAsync();
          NoteModel newNoteModel = NoteModelProvider.Resolve(newNoteId, () => NoteModelFactory.CreateDefault(newNoteId));
          NoteViewModel newNoteViewModel = NoteViewModelProvider.Resolve(newNoteModel);
          await NoteWindowService.OpenNoteWindow(newNoteModel);
        }
      }
    };

    ViewListCommand = new()
    {
      ActionToExecute = async (noteModel) =>
      {
        var mainWindow = await MainWindowService.GetOrCreate(noteModel.NavigationId);
        mainWindow.Activate();
      }
    };

    RemoveNoteCommand = new()
    {
      ActionToExecute = async (noteModel) =>
      {
        if (MainWindowService.TryGetCurrentWindow(out var mainWindow)
            && mainWindow.Content.XamlRoot is XamlRoot xamlRoot)
        {
          var deleteMode = DeleteMode.MoveToTrash;
          var result = await DialogService.ShowConfirmDeleteDialogAsync(xamlRoot, "Note", noteModel.Title, deleteMode);
          if (result.ContentDialogResult == ContentDialogResult.Primary)
          {
            switch (result.DeleteMode)
            {
              case DeleteMode.MoveToTrash:
                break;
              case DeleteMode.Permanent:
                await NoteService.DeleteNotePermanentlyAsync(noteModel.Id);
                break;
            }

            noteModel.IsDeleted = true;

            if (NavigationController.CurrentNavigation is INavigationNoteList navigation
                && NoteViewModelProvider.TryResolve(noteModel, out var noteViewModel))
            {
              WeakReferenceMessenger.Default.Send(new ValueChangedMessage<NoteViewModel>(noteViewModel), AppMessageTokens.RemoveNoteFromListToken(navigation));
            }
          }
        }
      }
    };

    AddNoteToJumpListCommand = new()
    {
      ActionToExecute = async (noteModel) => await JumpListService.AddToJumpListAsync(NoteMappers.ToDomain(noteModel))
    };
  }
}
