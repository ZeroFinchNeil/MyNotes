using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Application.Commands.Notes;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Results;
using MyNotes.Application.Services.Notes;
using MyNotes.Common.Commands;
using MyNotes.Common.Enums.Modes;
using MyNotes.Common.Structures;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Shell.Contracts.Converters;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private readonly NoteCommandService NoteCommandService;
  private readonly NoteService NoteService;
  private readonly SettingsService SettingsService;
  private readonly IRtfTextConverter RtfTextConverter;

  public NoteModel Note { get; }

  #region Object Lifetime Management
  public NoteViewModel([FromKeyedServices(CommandServiceType.Note)] ICommandService noteCommandService, NoteService noteService, SettingsService settingsService, IRtfTextConverter rtfTextConverter, NoteModel note)
  {
    // DI
    NoteCommandService = (NoteCommandService)noteCommandService;
    NoteService = noteService;
    SettingsService = settingsService;
    RtfTextConverter = rtfTextConverter;

    Note = note;

    SetBackgroundImage();
    Note.Preview = RtfTextConverter.GetPreview(Note.Body, 0, 500);
    Note.PropertyChanged += Note_PropertyChanged;
    SetCommands();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      _ = NoteService.Modification.CommitSearchIndexAsync();
    }

    base.Dispose(disposing);
  }
  #endregion

  #region Note 내부 속성 변경 시 데이터베이스에 반영 및 기타 로직 실행

  private async void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    // 뷰에 반영(TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.ShowBackgroundImage):
        if (Note.ShowBackgroundImage)
        {
          Note.BackdropKind = BackdropKind.None;
          SetBackgroundImage();
        }
        else
        {
          BackgroundImage = null;
        }
        await NoteService.Modification.UpdateNoteViewStateAsync(new UpdateNoteViewStateAppCommand()
        {
          PatchDto = new NoteViewStatePatchDto()
          {
            Id = Note.Id,
            ShowBackgroundImage = new(Note.ShowBackgroundImage)
          }
        });
        break;
      case nameof(Note.BackgroundImagePath):
        SetBackgroundImage();
        break;
    }
  }

  public async Task UpdateNoteTitle(string oldTitle)
  {
    UpdateNoteAppCommand appCommand = new()
    {
      PatchDto = new NotePatchDto()
      {
        Id = Note.Id,
        Title = new(Note.Title)
      }
    };
    var updateResult = await NoteService.Modification.UpdateNoteAsync(appCommand);
  }

  public async Task<bool> DeleteNotePermanentlyWhenEmpty()
  {
    if (SettingsService.Load(AppSettingsDescriptors.DeleteEmptyNote) && string.IsNullOrEmpty(Note.Title) && string.IsNullOrWhiteSpace(RtfTextConverter.ToPlainText(Note.Body)))
    {
      DeleteNoteAppCommand appCommand = new()
      {
        Id = Note.Id,
        DeleteMode = DeleteMode.Permanent
      };
      return await NoteService.Modification.DeleteNoteAsync(appCommand) is AppUpdateStatus.Succeeded;
    }

    return false;
  }
  #endregion

  #region Background Image

  [ObservableProperty]
  public partial BitmapImage? BackgroundImage { get; set; }

  private static BitmapImage? GetBackgroundImage(string? imagePath)
  {
    if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
    {
      return null;
    }

    try
    {
      BitmapImage image = new()
      {
        UriSource = new Uri(imagePath),
        DecodePixelType = DecodePixelType.Logical
      };
      return image;
    }
    catch (Exception)
    { }

    return null;
  }

  private void SetBackgroundImage() => BackgroundImage = Note.ShowBackgroundImage ? GetBackgroundImage(Note.BackgroundImagePath) : null;
  #endregion

  #region Body Images
  [ObservableProperty]
  public partial bool IsImagePanelVisible { get; set; }

  [ObservableProperty]
  public partial double ImagePanelMaxHeight { get; set; } = 120.0;
  #endregion
}

partial class NoteViewModel
{
  public Command<NoteModel> OpenWindowCommand => NoteCommandService.OpenNoteWindowCommand;
  public Command<NoteModel> MinimizeWindowCommand => NoteCommandService.MinimizeNoteWindowCommand;
  public Command<NoteModel> CloseWindowCommand => NoteCommandService.CloseNoteWindowCommand;
  public Command<SourceTargetPair<NoteModel, NavigationId>> MoveToListCommand => NoteCommandService.MoveNoteToListCommand;

  public AsyncCommand<NavigationId?> CreateNewNoteCommand => NoteCommandService.CreateNewNoteCommand;
  public Command<NoteModel> ViewListCommand => NoteCommandService.ViewListCommand;
  public AsyncCommand RenameNoteTitleCommand { get; private set; }
  public AsyncCommand<NoteModel> ToggleBookmarkNoteCommand => NoteCommandService.ToggleBookmarkNoteCommand;
  public AsyncCommand<NoteModel> RemoveNoteCommand => NoteCommandService.RemoveNoteCommand;

  public Command<NoteModel> AddNoteToJumpListCommand => NoteCommandService.AddNoteToJumpListCommand;

  public string OldTitle { get; set; } = string.Empty;

  [MemberNotNull(nameof(RenameNoteTitleCommand))]
  public void SetCommands()
  {
    RenameNoteTitleCommand = new()
    {
      ExecuteFunc = async () => await NoteCommandService.RenameNoteTitle(Note, OldTitle)
    };
  }
}