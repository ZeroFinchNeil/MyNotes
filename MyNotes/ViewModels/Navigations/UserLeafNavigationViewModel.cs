using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Models.Settings;
using MyNotes.Resources;
using MyNotes.Services.Commands;
using MyNotes.Services.Notes;
using MyNotes.Services.Window;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserLeafNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationViewModelCommandService NavigationViewModelCommandService;
  private readonly WindowService WindowService;
  private readonly NoteService NoteService;
  private readonly NoteViewModelProvider NoteViewModelProvider;

  public UserLeafNavigationViewModel([FromKeyedServices(CommandServiceType.NavigationViewModel)] ICommandService navigationViewModelCommandService, WindowService windowService, NoteService noteService, NoteViewModelProvider noteViewModelProvider, NavigationUserLeafNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelCommandService = (NavigationViewModelCommandService)navigationViewModelCommandService;
    WindowService = windowService;
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
      case nameof(NavigationUserCompositeNode.Icon):
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

  public override Command<NavigationViewModelBase>? AddListCommand => NavigationViewModelCommandService.AddListCommand;
  public override Command<NavigationViewModelBase>? AddGroupCommand => NavigationViewModelCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase>? UpdateCommand => NavigationViewModelCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase>? DeleteCommand => NavigationViewModelCommandService.DeleteCommand;
  public override Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)>? MoveToGroupCommand => NavigationViewModelCommandService.MoveToGroupCommand;

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
  public ObservableCollection<NoteViewModel> NoteViewModels { get; } = new();

  public NoteViewModelSortOrder NoteViewModelSortOrder
  {
    get;
    set => SetProperty(ref field, value);
  }

  public async Task LoadNoteViewModels()
  {
    var notes = await NoteService.GetNotesAsync(Navigation);
    foreach (var note in notes)
    {
      NoteViewModels.Add(NoteViewModelProvider.Resolve(note));
    }
  }

  public void UnloadNoteViewModels()
  {
    foreach (var noteViewModel in NoteViewModels)
    {
      if (!WindowService.NoteWindows.ContainsKey(noteViewModel.Note.Id))
        noteViewModel.Dispose();
    }
    NoteViewModels.Clear();
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
          NoteViewModels.Add(noteViewModel);

          NoteWindow noteWindow = new(note);
          noteWindow.Activate();
        }
      });
  }
}

  internal enum NoteViewModelSortOrder
{
  Title,
  Modified,
  Created,
}