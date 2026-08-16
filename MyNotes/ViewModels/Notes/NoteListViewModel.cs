using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Conditions;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Application.Navigations;
using MyNotes.Application.Navigations.Commands;
using MyNotes.Application.Navigations.Services;
using MyNotes.Application.Notes.Commands;
using MyNotes.Application.Notes.Services;
using MyNotes.Application.Results;
using MyNotes.Application.Settings.Services;
using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Helpers;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Debugging;
using MyNotes.Domain.Notes;
using MyNotes.Models;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.Templates;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteListViewModel : ViewModelBase, IAsyncDisposable
{
  private readonly AppSettingsService AppSettingsService;
  private readonly ViewStateSettingsService ViewStateSettingsService;
  private readonly NavigationService NavigationService;
  private readonly NoteService NoteService;
  private readonly NoteWindowService NoteWindowService;
  private readonly MainWindowService MainWindowService;
  private readonly IModelFactory<NoteDto, NoteModel> NoteModelFactory;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly INavigationNoteList Navigation;

  #region Object Lifetime Management
  public NoteListViewModel(AppSettingsService appSettingsService, ViewStateSettingsService viewStateSettingsService, NavigationService navigationService, NoteService noteService, NoteWindowService noteWindowService, MainWindowService mainWindowService, IModelFactory<NoteDto, NoteModel> noteModelFactory, NoteViewModelProvider noteViewModelProvider, INavigationNoteList navigation)
  {
    AppSettingsService = appSettingsService;
    ViewStateSettingsService = viewStateSettingsService;
    NavigationService = navigationService;
    NoteService = noteService;
    NoteWindowService = noteWindowService;
    MainWindowService = mainWindowService;
    NoteModelFactory = noteModelFactory;
    NoteViewModelProvider = noteViewModelProvider;

    Navigation = navigation;

    SetCommands();
    RegisterMessengers();

    LoadSortOrderAndPreviewStyle();
    _noteViewModelLeases = new(GetComparer(NoteSortKey, NoteSortDirection));
    InitializeTask = InitializeAsync();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {

    }

    base.Dispose(disposing);
  }

  bool _disposeStarted;
  private async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    await _noteViewModelLeases.DisposeAsync();
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
        //todo: 검색 조건에 따른 쿼리 구성
        NoteFilterDto noteFilterDto = new()
        {
          NoteFindFields = NoteFindFields.TitleConditions,
          TitleConditions = QueryConditionSet<StringQueryCondition>.Create(
            conditions: [StringQueryCondition.Create(target: search.SearchText, condition: TextMatchType.Contains)])
        };
        var searchResultDtos = await NoteService.Retrieval.SearchNotesAsync(noteFilterDto);
        if (searchResultDtos.Count == 0)
        {
          return;
        }

        foreach (var searchResultDto in searchResultDtos)
        {
          NoteModel searchedNote = NoteModelFactory.Create(searchResultDto);
          await _noteViewModelLeases.AddAsync(await NoteViewModelProvider.ResolveAsync(searchedNote));
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
  }

  //private async void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  //{
  //  if (sender is NoteModel note)
  //  {
  //    await using var lease = await NoteViewModelProvider.AcquireAsync(note);
  //    if (lease is null)
  //    {
  //      return;
  //    }
  //    var viewmodel = lease.ViewModel;
  //    switch (e.PropertyName)
  //    {
  //      case nameof(NoteModel.Title):
  //        _noteViewModelLeases.ReorderItem(viewmodel);
  //        break;
  //      case nameof(NoteModel.IsBookmarked):
  //        if (!note.IsBookmarked && Navigation is NavigationBookmarks)
  //        {
  //          await _noteViewModelLeases.RemoveAsync(viewmodel);
  //        }
  //        break;
  //    }
  //  }
  //}

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

  private NoteSortKey _noteSortKey;
  public NoteSortKey NoteSortKey
  {
    get => _noteSortKey;
    set
    {
      if (SetProperty(ref _noteSortKey, value))
      {
        if (Navigation is NavigationUserLeafNode
            && ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomNoteSortOrder))
        {
          Navigation.NoteSortKey = value;
          Navigation.NoteSortDirection = NoteSortDirection;
        }
        else
        {
          AppSettingsService.Save(NoteSortKeySettingsCodec.Encode, NavigationSettingsDescriptors.NoteSortKey, value);
        }
        _noteViewModelLeases.Rearrange(GetComparer(NoteSortKey, NoteSortDirection));
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
            && ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomNoteSortOrder))
        {
          Navigation.NoteSortKey = NoteSortKey;
          Navigation.NoteSortDirection = value;
        }
        else
        {
          AppSettingsService.Save(SortDirectionSettingsCodec.Encode, NavigationSettingsDescriptors.NoteSortDirection, value);
        }
        _noteViewModelLeases.Rearrange(GetComparer(NoteSortKey, NoteSortDirection));
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
            && ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = value;
          Navigation.PreviewTileSize = PreviewTileSize;
          Navigation.PreviewTileRatio = PreviewTileRatio;
        }
        else
        {
          AppSettingsService.Save(PreviewLayoutTypeSettingsCodec.Encode, NavigationSettingsDescriptors.PreviewLayoutType, value);
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
            && ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = PreviewLayoutType;
          Navigation.PreviewTileSize = value;
          Navigation.PreviewTileRatio = PreviewTileRatio;
        }
        else
        {
          AppSettingsService.Save(PreviewTileSizeSettingsCodec.Encode, NavigationSettingsDescriptors.PreviewTileSize, value);
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
            && ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = PreviewLayoutType;
          Navigation.PreviewTileSize = PreviewTileSize;
          Navigation.PreviewTileRatio = value;
        }
        else
        {
          AppSettingsService.Save(PreviewTileRatioSettingsCodec.Encode, NavigationSettingsDescriptors.PreviewTileRatio, value);
        }
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

  public void LoadSortOrderAndPreviewStyle()
  {
    if (ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomNoteSortOrder))
    {
      _noteSortKey = Navigation.NoteSortKey;
      _noteSortDirection = Navigation.NoteSortDirection;
    }
    else
    {
      _noteSortKey = AppSettingsService.Load<NoteSortKey, int>(NoteSortKeySettingsCodec.Decode, NavigationSettingsDescriptors.NoteSortKey);
      _noteSortDirection = AppSettingsService.Load<SortDirection, int>(SortDirectionSettingsCodec.Decode, NavigationSettingsDescriptors.NoteSortDirection);
    }

    if (ViewStateSettingsService.Load(ViewStateSettingsDescriptors.AllowCustomPreviewLayout))
    {
      _previewLayoutType = Navigation.PreviewLayoutType;
      _previewTileSize = Navigation.PreviewTileSize;
      _previewTileRatio = Navigation.PreviewTileRatio;
    }
    else
    {
      _previewLayoutType = AppSettingsService.Load<PreviewLayoutType, int>(PreviewLayoutTypeSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewLayoutType);
      _previewTileSize = AppSettingsService.Load<PreviewTileSize, int>(PreviewTileSizeSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewTileSize);
      _previewTileRatio = AppSettingsService.Load<PreviewTileRatio, int>(PreviewTileRatioSettingsCodec.Decode, NavigationSettingsDescriptors.PreviewTileRatio);
    }
  }
  //}

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
  public AsyncCommand? AddNoteCommand { get; private set; }

  private void SetCommands()
  {
    AddNoteCommand = new(
      executeFunc: async () =>
      {
        if (Navigation is NavigationUserLeafNode leaf)
        {
          var size = ViewStateSettingsService.Load<SizeInt32, Size>(s => new((int)s.Width, (int)s.Height), ViewStateSettingsDescriptors.NoteSize);
          var position = MainWindowService.GetNewWindowPosition(size) ?? ViewStateSettingsDescriptors.NoteWindowPosition.PointInt32;
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
      && NoteSortKey is NoteSortKey.Title)
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