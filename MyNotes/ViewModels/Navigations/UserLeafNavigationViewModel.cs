using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Models.Settings;
using MyNotes.Services.Commands;
using MyNotes.Services.Notes;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels.Notes;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserLeafNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationViewModelCommandService NavigationViewModelCommandService;
  private readonly WindowService WindowService;
  private readonly SettingsService SettingsService;
  private readonly NoteService NoteService;
  private readonly NoteViewModelProvider NoteViewModelProvider;

  public UserLeafNavigationViewModel([FromKeyedServices(CommandServiceType.NavigationViewModel)] ICommandService navigationViewModelCommandService, SettingsService settingsService, WindowService windowService, NoteService noteService, NoteViewModelProvider noteViewModelProvider, NavigationUserLeafNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelCommandService = (NavigationViewModelCommandService)navigationViewModelCommandService;
    WindowService = windowService;
    SettingsService = settingsService;
    NoteService = noteService;
    NoteViewModelProvider = noteViewModelProvider;

    SetIconImage();
    Navigation.PropertyChanged += Navigation_PropertyChanged;

    SetCommands();

    // Messengers
    RegisterMessenger();
  }

  private async void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(NavigationUserLeafNode.Icon):
        SetIconImage();
        break;
    }
  }

  private void SetIconImage() => IconImage = new BitmapImage() { UriSource = IconHelper.GetMainUri((short)Navigation.Icon) };

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      UnloadNoteViewModels();
      Navigation.PropertyChanged -= Navigation_PropertyChanged;
      UnregisterMessenger();
    }

    _disposed = true;
  }

  public override Command<NavigationViewModelBase> AddListCommand => NavigationViewModelCommandService.AddListCommand;
  public override Command<NavigationViewModelBase> AddGroupCommand => NavigationViewModelCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase> UpdateCommand => NavigationViewModelCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase> DeleteCommand => NavigationViewModelCommandService.DeleteCommand;
  public override Command<SourceTargetPair<NavigationViewModelBase, NavigationViewModelBase>> MoveToGroupCommand => NavigationViewModelCommandService.MoveToGroupCommand;
  public override Command<NavigationViewModelBase> SetAsStartPageCommand => NavigationViewModelCommandService.SetAsStartPageCommand;

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, MessageTokens.ChangeNavigationViewModelIconImageToken, (recipient, message) => SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}

internal sealed partial class UserLeafNavigationViewModel : UserNavigationViewModel
{
  public NoteViewModelCollection? NoteViewModels
  {
    get;
    private set => SetProperty(ref field, value);
  }

  private NoteSortKey _noteSortKey;
  public NoteSortKey NoteSortKey
  {
    get => _noteSortKey;
    set
    {
      if (_noteSortKey != value)
      {
        SetProperty(ref _noteSortKey, value);
        if (SettingsService.Load(SettingsDescriptors.AllowCustomNoteSortOrder))
        {
          Navigation.NoteSortKey = value;
          Navigation.NoteSortDirection = NoteSortDirection;
        }
        else
        {
          SettingsService.Save(SettingsDescriptors.NoteSortKey, (int)value);
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
      if (_noteSortDirection != value)
      {
        SetProperty(ref _noteSortDirection, value);
        if (SettingsService.Load(SettingsDescriptors.AllowCustomNoteSortOrder))
        {
          Navigation.NoteSortKey = NoteSortKey;
          Navigation.NoteSortDirection = value;
        }
        else
        {
          SettingsService.Save(SettingsDescriptors.NoteSortDirection, (int)value);
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
      if (_previewLayoutType != value)
      {
        SetProperty(ref _previewLayoutType, value);
        if (SettingsService.Load(SettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = value;
          Navigation.PreviewTileSize = PreviewTileSize;
          Navigation.PreviewTileRatio = PreviewTileRatio;
        }
        else
        {
          SettingsService.Save(SettingsDescriptors.PreviewLayoutType, (int)value);
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
      if (_previewTileSize != value)
      {
        SetProperty(ref _previewTileSize, value);
        if (SettingsService.Load(SettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = PreviewLayoutType;
          Navigation.PreviewTileSize = value;
          Navigation.PreviewTileRatio = PreviewTileRatio;
        }
        else
        {
          SettingsService.Save(SettingsDescriptors.PreviewTileSize, (int)value);
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
      if (_previewTileRatio != value)
      {
        SetProperty(ref _previewTileRatio, value);
        if (SettingsService.Load(SettingsDescriptors.AllowCustomPreviewLayout))
        {
          Navigation.PreviewLayoutType = PreviewLayoutType;
          Navigation.PreviewTileSize = PreviewTileSize;
          Navigation.PreviewTileRatio = value;
        }
        else
        {
          SettingsService.Save(SettingsDescriptors.PreviewTileRatio, (int)value);
        }
      }
    }
  }

  private static Comparer<Note> GetComparer(NoteSortKey noteSortKey, SortDirection sortDirection) => (noteSortKey, sortDirection) switch
  {
    (NoteSortKey.Modified, SortDirection.Ascending) => Comparer<Note>.Create((x, y) => x.Modified.CompareTo(y.Modified)),
    (NoteSortKey.Modified, SortDirection.Descending) => Comparer<Note>.Create((x, y) => y.Modified.CompareTo(x.Modified)),
    (NoteSortKey.Created, SortDirection.Ascending) => Comparer<Note>.Create((x, y) => x.Created.CompareTo(y.Created)),
    (NoteSortKey.Created, SortDirection.Descending) => Comparer<Note>.Create((x, y) => y.Created.CompareTo(x.Created)),
    (NoteSortKey.Title, SortDirection.Ascending) => Comparer<Note>.Create((x, y) => x.Title.CompareTo(y.Title)),
    (NoteSortKey.Title, SortDirection.Descending) => Comparer<Note>.Create((x, y) => y.Title.CompareTo(x.Title)),
    _ => throw new ArgumentException("Invalid sorting")
  };

  public void LoadSortOrderAndPreviewStyle()
  {
    var defaultNoteSortKey = (NoteSortKey)SettingsService.Load(SettingsDescriptors.NoteSortKey);
    var defaultNoteSortDirection = (SortDirection)SettingsService.Load(SettingsDescriptors.NoteSortDirection);
    if (SettingsService.Load(SettingsDescriptors.AllowCustomPreviewLayout))
    {
      _noteSortKey = Navigation.NoteSortKey ?? defaultNoteSortKey;
      _noteSortDirection = Navigation.NoteSortDirection ?? defaultNoteSortDirection;
    }
    else
    {
      _noteSortKey = defaultNoteSortKey;
      _noteSortDirection = defaultNoteSortDirection;
    }

    var defaultPreviewLayoutType = (PreviewLayoutType)SettingsService.Load(SettingsDescriptors.PreviewLayoutType);
    var defaultPreviewTileSize = (PreviewTileSize)SettingsService.Load(SettingsDescriptors.PreviewTileSize);
    var defaultPreviewTileRatio = (PreviewTileRatio)SettingsService.Load(SettingsDescriptors.PreviewTileRatio);
    if (SettingsService.Load(SettingsDescriptors.AllowCustomPreviewLayout))
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
    var notes = await NoteService.GetNotesAsync(Navigation);
    foreach (var note in notes)
    {
      note.PropertyChanged += Note_PropertyChanged_WhileActive;
      NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
    }
  }

  public void UnloadNoteViewModels()
  {
    if (NoteViewModels is null)
      return;

    foreach (var noteViewModel in NoteViewModels)
    {
      noteViewModel.Note.PropertyChanged -= Note_PropertyChanged_WhileActive;
      if (!WindowService.NoteWindows.ContainsKey(noteViewModel.Note.Id))
        noteViewModel.Dispose();
    }
    NoteViewModels.Clear();
    NoteViewModels = null;
  }

  private void Note_PropertyChanged_WhileActive(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is Note note)
    {
      if (e.PropertyName == nameof(Note.Title))
      {
        var viewmodel = NoteViewModelProvider.Resolve(note);
        NoteViewModels?.ReorderItem(viewmodel);
      }
    }
  }

  public Command? AddNoteCommand { get; private set; }

  private void SetCommands()
  {
    AddNoteCommand = new(
      actionToExecute: async () =>
      {
        if (await NoteService.AddNoteAsync(Navigation) is Note note)
        {
          NoteViewModel noteViewModel = NoteViewModelProvider.Resolve(note);
          NoteViewModels?.Add(noteViewModel);

          NoteService.OpenNoteWindow(note);
        }
      });
  }
}