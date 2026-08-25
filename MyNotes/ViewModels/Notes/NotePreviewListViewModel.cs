using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Application.Navigations.Commands;
using MyNotes.Application.Navigations.Services;
using MyNotes.Application.Notes.Commands;
using MyNotes.Application.Notes.Services;
using MyNotes.Application.Results;
using MyNotes.Application.Settings.Services;
using MyNotes.Common.Commands;
using MyNotes.Common.Helpers;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Domain.Notes;
using MyNotes.Models;
using MyNotes.Models.Navigations;
using MyNotes.Models.Navigations.Core;
using MyNotes.Models.Navigations.User;
using MyNotes.Models.Notes;
using MyNotes.Services.Windows;
using MyNotes.Templates;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NotePreviewListViewModel : ViewModelBase, IAsyncDisposable
{
  private readonly AppSettingsService AppSettingsService;
  private readonly NavigationService NavigationService;
  private readonly NoteService NoteService;
  private readonly NoteWindowService NoteWindowService;
  private readonly MainWindowService MainWindowService;
  private readonly IModelFactory<NoteDto, NoteModel> NoteModelFactory;
  private readonly NotePreviewViewModelProvider NotePreviewViewModelProvider;
  private readonly INavigationNoteList Navigation;

  #region Object Lifetime Management
  public NotePreviewListViewModel(AppSettingsService appSettingsService, NavigationService navigationService, NoteService noteService, NoteWindowService noteWindowService, MainWindowService mainWindowService, IModelFactory<NoteDto, NoteModel> noteModelFactory, NotePreviewViewModelProvider notePreviewViewModelProvider, INavigationNoteList navigation)
  {
    AppSettingsService = appSettingsService;
    NavigationService = navigationService;
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    MainWindowService = mainWindowService;
    NoteModelFactory = noteModelFactory;
    NotePreviewViewModelProvider = notePreviewViewModelProvider;

    Navigation = navigation;

    SetCommands();
    RegisterMessengers();

    _notePreviewViewModelLeases = new(GetComparer(Navigation.NoteSortKey, Navigation.NoteSortDirection));
    InitializeTask = InitializeAsync();
  }

  bool _disposeStarted;
  private async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    await _notePreviewViewModelLeases.DisposeAsync();
    Navigation.PropertyChanged -= Navigation_PropertyChanged;
    UnregisterMessengers();
  }
  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    Dispose(disposing: false);
  }
  #endregion

  private readonly LeasedNotePreviewViewModelCollection _notePreviewViewModelLeases;
  public IReadOnlyList<NotePreviewViewModel> NotePreviewViewModels => _notePreviewViewModelLeases.ViewModels;

  public Task InitializeTask { get; }
  public async Task InitializeAsync()
  {
    switch (Navigation)
    {
      case NavigationUserLeafNode leafNavigation:
        var leafNotes = (await NoteService.Retrieval.GetNotesByParentAsync(leafNavigation.Id, false)).Select(NoteModelFactory.Create);
        foreach (var note in leafNotes)
        {
          await _notePreviewViewModelLeases.AddAsync(await NotePreviewViewModelProvider.ResolveAsync(note, leafNavigation));
        }
        break;
      case NavigationSearch searchNavigation:
        await foreach (var noteSearchResultDto in NoteService.Retrieval.SearchNotesAsync(searchNavigation.SearchText))
        {
          var hitDto = noteSearchResultDto.HitDto;
          NoteModel searchedNote = NoteModelFactory.Create(noteSearchResultDto.NoteDto);
          var lease = await NotePreviewViewModelProvider.ResolveAsync(searchedNote, searchNavigation);
          await _notePreviewViewModelLeases.AddAsync(lease);
          (lease.ViewModel as NoteSearchPreviewViewModel)?.HighlightPreview(hitDto.BodyMatchRanges);
        }
        break;
      case NavigationBookmarks bookmarksNavigation:
        var bookmarkResultDtos = await NoteService.Retrieval.GetBookmarkedNotesAsync();
        foreach (var bookmarkResultDto in bookmarkResultDtos)
        {
          NoteModel bookmarkedNote = NoteModelFactory.Create(bookmarkResultDto);
          await _notePreviewViewModelLeases.AddAsync(await NotePreviewViewModelProvider.ResolveAsync(bookmarkedNote, bookmarksNavigation));
        }
        break;
      case NavigationTrash trashNavigation:
        var trashResultDtos = await NoteService.Retrieval.GetTrashedNotesAsync();
        foreach (var trashResultDto in trashResultDtos)
        {
          NoteModel trashedNote = NoteModelFactory.Create(trashResultDto);
          await _notePreviewViewModelLeases.AddAsync(await NotePreviewViewModelProvider.ResolveAsync(trashedNote, trashNavigation));
        }
        break;
    }

    Navigation.PropertyChanged += Navigation_PropertyChanged;
  }

  private void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(INavigationNoteList.NoteSortKey):
      case nameof(INavigationNoteList.NoteSortDirection):
        _notePreviewViewModelLeases.Rearrange(GetComparer(Navigation.NoteSortKey, Navigation.NoteSortDirection));
        break;
    }
  }

  [ObservableProperty]
  public partial Icon Icon { get; set; }

  async partial void OnIconChanged(Icon oldValue, Icon newValue)
  {
    if (Navigation is NavigationUserNode navigationUserNode)
    {
      var updateResult = await NavigationService.Modification.UpdateNavigationAsync(new UpdateNavigationAppCommand()
      {
        PatchDto = new NavigationPatchDto()
        {
          Id = navigationUserNode.Id,
          Icon = (int)newValue
        }
      });
      if (updateResult is AppUpdateStatus.Succeeded)
      {
        navigationUserNode.Icon = newValue;
      }
    }
  }

  private static Comparer<NotePreviewViewModel> GetComparer(NoteSortKey noteSortKey, SortDirection sortDirection) => (noteSortKey, sortDirection) switch
  {
    (NoteSortKey.Modified, SortDirection.Ascending) => Comparer<NotePreviewViewModel>.Create((x, y) => x.Note.Modified.CompareTo(y.Note.Modified)),
    (NoteSortKey.Modified, SortDirection.Descending) => Comparer<NotePreviewViewModel>.Create((x, y) => y.Note.Modified.CompareTo(x.Note.Modified)),
    (NoteSortKey.Created, SortDirection.Ascending) => Comparer<NotePreviewViewModel>.Create((x, y) => x.Note.Created.CompareTo(y.Note.Created)),
    (NoteSortKey.Created, SortDirection.Descending) => Comparer<NotePreviewViewModel>.Create((x, y) => y.Note.Created.CompareTo(x.Note.Created)),
    (NoteSortKey.Title, SortDirection.Ascending) => Comparer<NotePreviewViewModel>.Create((x, y) => x.Note.Title.CompareTo(y.Note.Title)),
    (NoteSortKey.Title, SortDirection.Descending) => Comparer<NotePreviewViewModel>.Create((x, y) => y.Note.Title.CompareTo(x.Note.Title)),
    _ => throw new ArgumentException("Invalid sorting")
  };

  public async Task RemoveNoteFromListAsync(NoteId noteId)
  {
    if (NotePreviewViewModels.FirstOrDefault(vm => vm.Note.Id == noteId) is NotePreviewViewModel viewmodel)
    {
      await _notePreviewViewModelLeases.RemoveAsync(viewmodel);
    }
  }
}

#region Commands and Messengers
partial class NotePreviewListViewModel
{
  public AsyncCommand? AddNoteCommand { get; private set; }

  private void SetCommands()
  {
    AddNoteCommand = new(
      executeFunc: async () =>
      {
        if (Navigation is NavigationUserLeafNode leafNavigation)
        {
          var size = AppSettingsService.Load<SizeInt32, Size>(s => new((int)s.Width, (int)s.Height), AppSettingsDescriptors.DefaultNoteSize);
          var position = MainWindowService.GetNewWindowPosition(size) ?? AppSettingsDescriptors.DefaultNoteWindowPosition.PointInt32;
          CreateNoteAppCommand createNoteAppCommand = new()
          {
            NavigationId = leafNavigation.Id,
            Size = size,
            Position = position
          };
          var bundleDto = await NoteService.Creation.AddNoteAsync(createNoteAppCommand);
          if (bundleDto is null)
          {
            return;
          }

          NoteModel noteModel = NoteModelFactory.Create(bundleDto);
          await _notePreviewViewModelLeases.AddAsync(await NotePreviewViewModelProvider.ResolveAsync(noteModel, leafNavigation));
          await NoteWindowService.OpenNoteWindow(noteModel);
        }
      });
  }

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<NoteModel>, MessageToken>(this, AppMessageTokens.NoteTitleChangedToken, (recipient, message) =>
    {
      if (NotePreviewViewModels.FirstOrDefault(vm => vm.Note == message.Value) is NotePreviewViewModel viewmodel
      && Navigation.NoteSortKey is NoteSortKey.Title)
      {
        _notePreviewViewModelLeases.ReorderItem(viewmodel);
      }
    });

    WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, MessageToken>(this, AppMessageTokens.ChangeNoteIsBookmarkedStateToken, async (recipient, message) =>
    {
      if (message.Sender is NoteModel targetNote)
      {
        switch (Navigation)
        {
          case NavigationBookmarks bookmarksNavigation:
            var noteViewModel = NotePreviewViewModels.FirstOrDefault(vm => vm.Note.Id == targetNote.Id);
            if (message.NewValue)
            {
              if (noteViewModel is null)
              {
                await _notePreviewViewModelLeases.AddAsync(await NotePreviewViewModelProvider.ResolveAsync(targetNote, bookmarksNavigation));
              }
            }
            else
            {
              if (noteViewModel is not null)
              {
                await _notePreviewViewModelLeases.RemoveAsync(noteViewModel);
              }
            }
            break;
        }
      }
    });

    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NoteId, bool>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.IsNoteInListToken(Navigation), (recipient, message) =>
    {
      message.Reply(NotePreviewViewModels.FirstOrDefault(vm => vm.Note.Id == message.Request) is not null);
    });

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<NoteModel>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.AddNoteToListToken(Navigation), async (recipient, message) =>
    {
      NoteModel note = message.Value;
      await _notePreviewViewModelLeases.AddAsync(await NotePreviewViewModelProvider.ResolveAsync(note, Navigation));
    });
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
#endregion