using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Dtos.Notes.Queries;
using MyNotes.Application.Services.Notes;
using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Mappers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Shared.Queries.Conditions;
using MyNotes.Shared.Queries.Enums;
using MyNotes.Shell.Contracts.Windowing;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteListViewModel : ViewModelBase
{
  private readonly SettingsService SettingsService;
  private readonly NoteService NoteService;
  private readonly NoteWindowService NoteWindowService;
  private readonly NoteModelFactory NoteModelFactory;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly INavigationNoteList Navigation;

  #region Object Lifetime Management
  public NoteListViewModel(INativeWindowing nativeWindowing, SettingsService settingsService, NoteService noteService, NoteWindowService noteWindowService, NoteModelFactory noteModelFactory, NoteViewModelProvider noteViewModelProvider, INavigationNoteList navigation)
  {
    SettingsService = settingsService;
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    NoteModelFactory = noteModelFactory;
    NoteViewModelProvider = noteViewModelProvider;

    Navigation = navigation;

    SetCommands();
    RegisterMessengers();

    LoadSortOrderAndPreviewStyle();
    _ = LoadNoteViewModels();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      UnregisterMessengers();
      UnloadNoteViewModels();
    }

    base.Dispose(disposing);
  }
  #endregion

  [ObservableProperty]
  public partial NoteViewModelCollection? NoteViewModels { get; private set; }

  private NoteSortKey _noteSortKey;
  public NoteSortKey NoteSortKey
  {
    get => _noteSortKey;
    set
    {
      if (SetProperty(ref _noteSortKey, value))
      {
        if (Navigation is NavigationUserLeafNode
            && SettingsService.Load(AppSettingsDescriptors.AllowCustomNoteSortOrder))
        {
          Navigation.NoteSortKey = value;
          Navigation.NoteSortDirection = NoteSortDirection;
        }
        else
        {
          SettingsService.Save(AppSettingsDescriptors.NoteSortKey, (int)value);
        }
        var comparer = GetComparer(NoteSortKey, NoteSortDirection);
        NoteViewModels = NoteViewModels is null ? new(comparer) : new(NoteViewModels, comparer);
      }
    }
  }

  private SortDirection _noteSortDirection;
  public SortDirection NoteSortDirection
  {
    get => _noteSortDirection;
    set
    {
      if (SetProperty(ref _noteSortDirection, value))
      {
        if (Navigation is NavigationUserLeafNode
            && SettingsService.Load(AppSettingsDescriptors.AllowCustomNoteSortOrder))
        {
          Navigation.NoteSortKey = NoteSortKey;
          Navigation.NoteSortDirection = value;
        }
        else
        {
          SettingsService.Save(AppSettingsDescriptors.NoteSortDirection, (int)value);
        }
        var comparer = GetComparer(NoteSortKey, NoteSortDirection);
        NoteViewModels = NoteViewModels is null ? new(comparer) : new(NoteViewModels, comparer);
      }
    }
  }

  private PreviewLayoutType _previewLayoutType;
  public PreviewLayoutType PreviewLayoutType
  {
    get => _previewLayoutType;
    set
    {
      if (SetProperty(ref _previewLayoutType, value))
      {
        if (Navigation is NavigationUserLeafNode
            && SettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = value;
          Navigation.PreviewTileSize = PreviewTileSize;
          Navigation.PreviewTileRatio = PreviewTileRatio;
        }
        else
        {
          SettingsService.Save(AppSettingsDescriptors.PreviewLayoutType, (int)value);
        }
      }
    }
  }

  private PreviewTileSize _previewTileSize;
  public PreviewTileSize PreviewTileSize
  {
    get => _previewTileSize;
    set
    {
      if (SetProperty(ref _previewTileSize, value))
      {
        if (Navigation is NavigationUserLeafNode
            && SettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = PreviewLayoutType;
          Navigation.PreviewTileSize = value;
          Navigation.PreviewTileRatio = PreviewTileRatio;
        }
        else
        {
          SettingsService.Save(AppSettingsDescriptors.PreviewTileSize, (int)value);
        }
      }
    }
  }

  private PreviewTileRatio _previewTileRatio;
  public PreviewTileRatio PreviewTileRatio
  {
    get => _previewTileRatio;
    set
    {
      if (SetProperty(ref _previewTileRatio, value))
      {
        if (Navigation is NavigationUserLeafNode
            && SettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = PreviewLayoutType;
          Navigation.PreviewTileSize = PreviewTileSize;
          Navigation.PreviewTileRatio = value;
        }
        else
        {
          SettingsService.Save(AppSettingsDescriptors.PreviewTileRatio, (int)value);
        }
      }
    }
  }

  private static Comparer<NoteModel> GetComparer(NoteSortKey noteSortKey, SortDirection sortDirection) => (noteSortKey, sortDirection) switch
  {
    (NoteSortKey.Modified, SortDirection.Ascending) => Comparer<NoteModel>.Create((x, y) => x.Modified.CompareTo(y.Modified)),
    (NoteSortKey.Modified, SortDirection.Descending) => Comparer<NoteModel>.Create((x, y) => y.Modified.CompareTo(x.Modified)),
    (NoteSortKey.Created, SortDirection.Ascending) => Comparer<NoteModel>.Create((x, y) => x.Created.CompareTo(y.Created)),
    (NoteSortKey.Created, SortDirection.Descending) => Comparer<NoteModel>.Create((x, y) => y.Created.CompareTo(x.Created)),
    (NoteSortKey.Title, SortDirection.Ascending) => Comparer<NoteModel>.Create((x, y) => x.Title.CompareTo(y.Title)),
    (NoteSortKey.Title, SortDirection.Descending) => Comparer<NoteModel>.Create((x, y) => y.Title.CompareTo(x.Title)),
    _ => throw new ArgumentException("Invalid sorting")
  };

  public void LoadSortOrderAndPreviewStyle()
  {
    var defaultNoteSortKey = (NoteSortKey)SettingsService.Load(AppSettingsDescriptors.NoteSortKey);
    var defaultNoteSortDirection = (SortDirection)SettingsService.Load(AppSettingsDescriptors.NoteSortDirection);
    if (SettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout))
    {
      _noteSortKey = Navigation.NoteSortKey ?? defaultNoteSortKey;
      _noteSortDirection = Navigation.NoteSortDirection ?? defaultNoteSortDirection;
    }
    else
    {
      _noteSortKey = defaultNoteSortKey;
      _noteSortDirection = defaultNoteSortDirection;
    }

    var defaultPreviewLayoutType = (PreviewLayoutType)SettingsService.Load(AppSettingsDescriptors.PreviewLayoutType);
    var defaultPreviewTileSize = (PreviewTileSize)SettingsService.Load(AppSettingsDescriptors.PreviewTileSize);
    var defaultPreviewTileRatio = (PreviewTileRatio)SettingsService.Load(AppSettingsDescriptors.PreviewTileRatio);
    if (SettingsService.Load(AppSettingsDescriptors.AllowCustomPreviewLayout))
    {
      _previewLayoutType = Navigation.PreviewLayoutType ?? defaultPreviewLayoutType;
      _previewTileSize = Navigation.PreviewTileSize ?? defaultPreviewTileSize;
      _previewTileRatio = Navigation.PreviewTileRatio ?? defaultPreviewTileRatio;
    }
    else
    {
      _previewLayoutType = defaultPreviewLayoutType;
      _previewTileSize = defaultPreviewTileSize;
      _previewTileRatio = defaultPreviewTileRatio;
    }
  }

  public async Task LoadNoteViewModels()
  {
    NoteViewModels = new(GetComparer(NoteSortKey, NoteSortDirection));
    switch (Navigation)
    {
      case NavigationUserLeafNode leaf:
        var leafNotes = (await NoteService.Retrieval.GetNotesByParentAsync(leaf.Id, false)).Select(NoteMappers.ToModel);
        foreach (var note in leafNotes)
        {
          note.PropertyChanged += Note_PropertyChanged_WhileActive;
          NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
        }
        break;
      case NavigationSearch search:
        SearchNotesAppQuery searchNotesAppQuery = new()
        {
          TitleConditions = new QueryConditionSet<string, TitleQueryCondition>()
          {
            Key = search.SearchText,
            Conditions = [new TitleMatchTypeQueryCondition() { Condition = TitleMatchType.Contains }]
          }
        };
        var searchResults = await NoteService.Retrieval.SearchNotesAsync(searchNotesAppQuery);
        if (searchResults.Count == 0)
        {
          return;
        }

        foreach(var searchResult in searchResults)
        {
          NoteId noteId = searchResult.Id;
          NoteModelFactory.GetOrCreate(noteId)
          note.PropertyChanged += Note_PropertyChanged_WhileActive;
          NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
          NoteMappers.ToModel()
        }

        await foreach (var match in searchResults.Matches)
        {
          if (await NoteService.Retrieval.FindNoteAsync(NoteId.Create(match.NoteId)) is NoteModel note)
          {
            note.PropertyChanged += Note_PropertyChanged_WhileActive;
            NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
          }
        }
        break;
      case NavigationBookmarks bookmarks:
        var bookmarksNotes = await NoteService.Retrieval.GetNotesAsync(e => e.IsBookmarked && !e.IsDeleted);
        foreach (var note in bookmarksNotes)
        {
          note.PropertyChanged += Note_PropertyChanged_WhileActive;
          NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
        }
        break;
      case NavigationTrash trash:
        var trashNotes = await NoteService.Retrieval.GetNotesAsync(e => e.IsDeleted);
        foreach (var note in trashNotes)
        {
          note.PropertyChanged += Note_PropertyChanged_WhileActive;
          NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
        }
        break;
    }

    NoteViewModels.CollectionChanged += NoteViewModels_CollectionChanged;
  }

  private void NoteViewModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems is IList removedItems)
    {
      foreach (var removed in removedItems)
      {
        (removed as NoteViewModel)?.Note.PropertyChanged -= Note_PropertyChanged_WhileActive;
      }
    }
    if (e.NewItems is IList addedItems)
    {
      foreach (var added in addedItems)
      {
        (added as NoteViewModel)?.Note.PropertyChanged -= Note_PropertyChanged_WhileActive;
        (added as NoteViewModel)?.Note.PropertyChanged += Note_PropertyChanged_WhileActive;
      }
    }
  }

  public void UnloadNoteViewModels()
  {
    if (NoteViewModels is null)
    {
      return;
    }

    NoteViewModels.CollectionChanged -= NoteViewModels_CollectionChanged;

    foreach (var noteViewModel in NoteViewModels)
    {
      noteViewModel.Note.PropertyChanged -= Note_PropertyChanged_WhileActive;
      NoteViewModelProvider.Release(noteViewModel.Note);
    }
    NoteViewModels.Clear();
    NoteViewModels = null;
  }

  private void Note_PropertyChanged_WhileActive(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is NoteModel note
        && NoteViewModelProvider.TryResolve(note, out var noteViewModel))
    {
      switch (e.PropertyName)
      {
        case nameof(NoteModel.Title):
          NoteViewModels?.ReorderItem(noteViewModel);
          break;
        case nameof(NoteModel.IsBookmarked):
          if (!note.IsBookmarked && Navigation is NavigationBookmarks)
          {
            NoteViewModels?.Remove(noteViewModel);
          }
          break;
      }
    }
  }

  public static readonly BijectiveMap<PreviewLayoutType, int> _previewLayoutTypeMap = new()
  {
    { PreviewLayoutType.Grid, (int)PreviewLayoutType.Grid },
    { PreviewLayoutType.List, (int)PreviewLayoutType.List },
  };
  public static IReadOnlyBijectiveMap<PreviewLayoutType, int> PreviewLayoutTypeMap => _previewLayoutTypeMap;

  public static readonly BijectiveMap<PreviewTileSize, double> _previewTileSizeMap = new()
  {
    { PreviewTileSize.Smallest, 120 },
    { PreviewTileSize.Smaller, 160 },
    { PreviewTileSize.Small, 200 },
    { PreviewTileSize.Medium, 240 },
    { PreviewTileSize.Large, 280 },
    { PreviewTileSize.Larger, 320 },
    { PreviewTileSize.Largest, 360 },
  };
  public static IReadOnlyBijectiveMap<PreviewTileSize, double> PreviewTileSizeMap => _previewTileSizeMap;

  public static readonly BijectiveMap<PreviewTileRatio, double> _previewTileRatioMap = new()
  {
    { PreviewTileRatio.Shorter, 0.50 },
    { PreviewTileRatio.Short, 0.75 },
    { PreviewTileRatio.Square, 1.00 },
    { PreviewTileRatio.Tall, 1.25 },
    { PreviewTileRatio.Taller, 1.50 },
  };
  public static IReadOnlyBijectiveMap<PreviewTileRatio, double> PreviewTileRatioMap => _previewTileRatioMap;

  public bool Equals(NoteSortKey key1, NoteSortKey key2) => key1 == key2;
  public bool Equals(SortDirection key1, SortDirection key2) => key1 == key2;
  public Visibility VisibleWhenEquals(PreviewLayoutType key1, PreviewLayoutType key2) => key1 == key2 ? Visibility.Visible : Visibility.Collapsed;

  public int ToInt(PreviewLayoutType type) => PreviewLayoutTypeMap.RightFromLeft(type);
  public double ToDouble(PreviewTileSize size) => PreviewTileSizeMap.RightFromLeft(size);
  public double ToDouble(PreviewTileRatio ratio) => PreviewTileRatioMap.RightFromLeft(ratio);

  public PreviewLayoutType ToPreviewLayoutType(int index, Action<PreviewLayoutType> action)
  {
    var previewLayoutType = PreviewLayoutTypeMap.LeftFromRight(index);
    action.Invoke(previewLayoutType);
    return previewLayoutType;
  }

  public PreviewTileSize ToPreviewTileSize(double value, Action<PreviewTileSize> action)
  {
    var previewTileSize = PreviewTileSizeMap.LeftFromRight(value);
    action.Invoke(previewTileSize);
    return previewTileSize;
  }

  public PreviewTileRatio ToPreviewTileRatio(double value, Action<PreviewTileRatio> action)
  {
    var previewTileRatio = PreviewTileRatioMap.LeftFromRight(value);
    action.Invoke(previewTileRatio);
    return previewTileRatio;
  }

  public void ChangePreviewLayout(ListViewBase listViewBase)
  {
    if (PreviewLayoutType is PreviewLayoutType.Grid)
    {
      listViewBase.ItemsPanel = App.Instance.Resources["NoteList_GridViewItemsPanel_LayoutGrid"] as ItemsPanelTemplate;
      listViewBase.ItemTemplate = App.Instance.Resources["NoteList_GridViewItemTemplate_LayoutGrid"] as DataTemplate;
    }
    else if (PreviewLayoutType is PreviewLayoutType.List)
    {
      listViewBase.ItemsPanel = App.Instance.Resources["NoteList_GridViewItemsPanel_LayoutList"] as ItemsPanelTemplate;
      listViewBase.ItemTemplate = App.Instance.Resources["NoteList_GridViewItemTemplate_LayoutList"] as DataTemplate;
    }
    ChangePreviewTile(listViewBase);
  }

  public void ChangePreviewTile(ListViewBase listViewBase)
  {
    var size = PreviewTileSizeMap.RightFromLeft(PreviewTileSize);
    var ratio = PreviewTileRatioMap.RightFromLeft(PreviewTileRatio);

    if (App.Instance.Resources["NoteList_GridViewItemContainerStyle"] is Style defaultStyle)
    {
      Style style = new() { TargetType = typeof(GridViewItem), BasedOn = defaultStyle };
      if (PreviewLayoutType is PreviewLayoutType.Grid)
      {
        style.Setters.Add(new Setter() { Property = FrameworkElement.WidthProperty, Value = size });
        style.Setters.Add(new Setter() { Property = FrameworkElement.HeightProperty, Value = size * ratio });
      }
      else if (PreviewLayoutType is PreviewLayoutType.List)
      {
        style.Setters.Add(new Setter() { Property = FrameworkElement.HeightProperty, Value = size * 0.625 });
      }

      listViewBase.ItemContainerStyle = style;
    }
  }
}

#region Commands and Messengers
partial class NoteListViewModel
{
  public Command? AddNoteCommand { get; private set; }

  private void SetCommands()
  {
    AddNoteCommand = new(
      executeAction: async () =>
      {
#if false
        if (Navigation is NavigationUserLeafNode leaf
            && await NoteService.Creation.AddNoteAsync(leaf) is NoteModel note)
        {
          NoteViewModel noteViewModel = NoteViewModelProvider.Resolve(note);
          NoteViewModels?.Add(noteViewModel);

          await NoteService.Retrieval.OpenNoteWindow(note);
        }
#endif
        throw new NotImplementedException();

      });
  }

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, MessageToken>(this, AppMessageTokens.ChangeNoteIsBookmarkedStateToken, (recipient, message) =>
    {
      if (message.Sender is NoteModel targetNote)
      {
        switch (Navigation)
        {
          case NavigationBookmarks:
            var noteViewModel = NoteViewModels?.FirstOrDefault(vm => vm.Note.Id == targetNote.Id);
            if (message.NewValue)
            {
              if (noteViewModel is null)
              {
                NoteViewModels?.Add(NoteViewModelProvider.Resolve(targetNote));
              }
            }
            else
            {
              if (noteViewModel is not null)
              {
                NoteViewModels?.Remove(noteViewModel);
              }
            }
            break;
        }
      }
    });

    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NoteId, bool>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.IsNoteInListToken(Navigation), (recipient, message) =>
    {
      message.Reply(NoteViewModels?.FirstOrDefault(vm => vm.Note.Id == message.Request) is not null);
    });

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<NoteViewModel>, MessageToken<INavigationNoteList>>(this, AppMessageTokens.RemoveNoteFromListToken(Navigation), (recipient, message) =>
    {
      NoteViewModels?.Remove(message.Value);
    });
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
#endregion