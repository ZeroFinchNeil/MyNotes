using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Conditions;
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
using MyNotes.Debugging;
using MyNotes.Domain.Notes;
using MyNotes.Models;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Windows;
using MyNotes.Templates;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteListViewModel : ViewModelBase, IAsyncDisposable
{
  private readonly AppSettingsService AppSettingsService;
  private readonly NavigationService NavigationService;
  private readonly NoteService NoteService;
  private readonly NoteWindowService NoteWindowService;
  private readonly MainWindowService MainWindowService;
  private readonly IModelFactory<NoteDto, NoteModel> NoteModelFactory;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly INavigationNoteList Navigation;

  #region Object Lifetime Management
  public NoteListViewModel(AppSettingsService appSettingsService, NavigationService navigationService, NoteService noteService, NoteWindowService noteWindowService, MainWindowService mainWindowService, IModelFactory<NoteDto, NoteModel> noteModelFactory, NoteViewModelProvider noteViewModelProvider, INavigationNoteList navigation)
  {
    AppSettingsService = appSettingsService;
    NavigationService = navigationService;
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    MainWindowService = mainWindowService;
    NoteModelFactory = noteModelFactory;
    NoteViewModelProvider = noteViewModelProvider;

    Navigation = navigation;

    SetCommands();
    RegisterMessengers();

    _noteViewModelLeases = new(GetComparer(Navigation.NoteSortKey, Navigation.NoteSortDirection));
    InitializeTask = InitializeAsync();
  }

  bool _disposeStarted;
  private async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    foreach(var noteViewModel in NoteViewModels)
    {
      noteViewModel.ResetHighlight();
    }
    await _noteViewModelLeases.DisposeAsync();
    Navigation.PropertyChanged -= Navigation_PropertyChanged;
    UnregisterMessengers();
  }
  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    Dispose(disposing: false);
  }
  #endregion

  private readonly LeasedNoteViewModelCollection _noteViewModelLeases;
  public IReadOnlyList<NoteViewModel> NoteViewModels => _noteViewModelLeases.ViewModels;

  public Task InitializeTask { get; }
  public async Task InitializeAsync()
  {
    switch (Navigation)
    {
      case NavigationUserLeafNode leaf:
        var leafNotes = (await NoteService.Retrieval.GetNotesByParentAsync(leaf.Id, false)).Select(NoteModelFactory.Create);
        foreach (var note in leafNotes)
        {
          await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(note));
        }
        break;
      case NavigationSearch search:
        await foreach (var noteSearchResultDto in NoteService.Retrieval.SearchNotesAsync(search.SearchText))
        {
          var hitDto = noteSearchResultDto.HitDto;
          NoteModel searchedNote = NoteModelFactory.Create(noteSearchResultDto.NoteDto);
          var lease = await NoteViewModelProvider.ResolveAsync(searchedNote);
          await _noteViewModelLeases.AddAsync(lease);
          lease.ViewModel.HighlightPreview(hitDto.BodyMatchRanges);
        }
        break;
      case NavigationBookmarks bookmarks:
        var bookmarkResultDtos = await NoteService.Retrieval.GetBookmarkedNotesAsync();
        foreach (var bookmarkResultDto in bookmarkResultDtos)
        {
          NoteModel bookmarkedNote = NoteModelFactory.Create(bookmarkResultDto);
          await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(bookmarkedNote));
        }
        break;
      case NavigationTrash trash:
        var trashResultDtos = await NoteService.Retrieval.GetTrashedNotesAsync();
        foreach (var trashResultDto in trashResultDtos)
        {
          NoteModel trashedNote = NoteModelFactory.Create(trashResultDto);
          await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(trashedNote));
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
        _noteViewModelLeases.Rearrange(GetComparer(Navigation.NoteSortKey, Navigation.NoteSortDirection));
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

  private static Comparer<NoteViewModel> GetComparer(NoteSortKey noteSortKey, SortDirection sortDirection) => (noteSortKey, sortDirection) switch
  {
    (NoteSortKey.Modified, SortDirection.Ascending) => Comparer<NoteViewModel>.Create((x, y) => x.Note.Modified.CompareTo(y.Note.Modified)),
    (NoteSortKey.Modified, SortDirection.Descending) => Comparer<NoteViewModel>.Create((x, y) => y.Note.Modified.CompareTo(x.Note.Modified)),
    (NoteSortKey.Created, SortDirection.Ascending) => Comparer<NoteViewModel>.Create((x, y) => x.Note.Created.CompareTo(y.Note.Created)),
    (NoteSortKey.Created, SortDirection.Descending) => Comparer<NoteViewModel>.Create((x, y) => y.Note.Created.CompareTo(x.Note.Created)),
    (NoteSortKey.Title, SortDirection.Ascending) => Comparer<NoteViewModel>.Create((x, y) => x.Note.Title.CompareTo(y.Note.Title)),
    (NoteSortKey.Title, SortDirection.Descending) => Comparer<NoteViewModel>.Create((x, y) => y.Note.Title.CompareTo(x.Note.Title)),
    _ => throw new ArgumentException("Invalid sorting")
  };
}

#region Commands and Messengers
partial class NoteListViewModel
{
  public AsyncCommand? AddNoteCommand { get; private set; }

  private void SetCommands()
  {
    AddNoteCommand = new(
      executeFunc: async () =>
      {
        if (Navigation is NavigationUserLeafNode leaf)
        {
          var size = AppSettingsService.Load<SizeInt32, Size>(s => new((int)s.Width, (int)s.Height), AppSettingsDescriptors.DefaultNoteSize);
          var position = MainWindowService.GetNewWindowPosition(size) ?? AppSettingsDescriptors.DefaultNoteWindowPosition.PointInt32;
          CreateNoteAppCommand createNoteAppCommand = new()
          {
            NavigationId = leaf.Id,
            Size = size,
            Position = position
          };
          var bundleDto = await NoteService.Creation.AddNoteAsync(createNoteAppCommand);
          if (bundleDto is null)
          {
            return;
          }

          NoteModel noteModel = NoteModelFactory.Create(bundleDto);
          await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(noteModel));
          await NoteWindowService.OpenNoteWindow(noteModel);
        }
      });
  }

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<NoteModel>, MessageToken>(this, AppMessageTokens.NoteTitleChangedToken, (recipient, message) =>
    {
      if (NoteViewModels.FirstOrDefault(vm => vm.Note == message.Value) is NoteViewModel viewmodel
      && Navigation.NoteSortKey is NoteSortKey.Title)
      {
        _noteViewModelLeases.ReorderItem(viewmodel);
      }
    });

    WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, MessageToken>(this, AppMessageTokens.ChangeNoteIsBookmarkedStateToken, async (recipient, message) =>
    {
      if (message.Sender is NoteModel targetNote)
      {
        switch (Navigation)
        {
          case NavigationBookmarks:
            var noteViewModel = NoteViewModels.FirstOrDefault(vm => vm.Note.Id == targetNote.Id);
            if (message.NewValue)
            {
              if (noteViewModel is null)
              {
                await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(targetNote));
              }
            }
            else
            {
              if (noteViewModel is not null)
              {
                await _noteViewModelLeases.RemoveAsync(noteViewModel);
              }
            }
            break;
        }
      }
    });

    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NoteId, bool>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.IsNoteInListToken(Navigation), (recipient, message) =>
    {
      message.Reply(NoteViewModels.FirstOrDefault(vm => vm.Note.Id == message.Request) is not null);
    });

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<NoteModel>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.AddNoteToListToken(Navigation), async (recipient, message) =>
    {
      NoteModel note = message.Value;
      await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(note));
    });

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<NoteModel>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.RemoveNoteFromListToken(Navigation), async (recipient, message) =>
    {
      NoteModel note = message.Value;
      if (NoteViewModels.FirstOrDefault(vm => vm.Note == note) is NoteViewModel viewmodel)
      {
        await _noteViewModelLeases.RemoveAsync(viewmodel);
      }
    });
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
#endregion