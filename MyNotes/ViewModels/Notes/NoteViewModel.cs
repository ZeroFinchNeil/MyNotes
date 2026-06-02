using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Application.Services.App;
using MyNotes.Application.Services.Notes;
using MyNotes.Common.Commands;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Common.Messages;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Windows;
using MyNotes.Templates.Media;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteViewModel : ViewModelBase
{
  private readonly NoteWindowService NoteWindowService;
  private readonly NoteCommandService NoteCommandService;
  private readonly NoteService NoteService;
  private readonly JumpListService JumpListService;

  public NoteModel Note { get; }

  #region Object Lifetime Management
  public NoteViewModel(NoteWindowService noteWindowService, [FromKeyedServices(CommandServiceType.Note)] ICommandService noteCommandService, NoteService noteService, JumpListService jumpListService, NoteModel note)
  {
    // DI
    NoteWindowService = noteWindowService;
    NoteCommandService = (NoteCommandService)noteCommandService;
    NoteService = noteService;
    JumpListService = jumpListService;

    Note = note;

    _notePropertyDebounceTimer.Elapsed += NotePropertyChangedDebounceTimer_Elapsed;

    _selectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.BackgroundColor);
    BackgroundImage = Note.ShowBackgroundImage ? GetBackgroundImage(Note.BackgroundImagePath) : null;
    Preview = GetPreview(Note.Body, 0, PreviewTextMaxLength);
    Note.PropertyChanged += Note_PropertyChanged;
    RegisterMessengers();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      UnregisterEventHandlers();
      _notePropertyDebounceTimer.Dispose();

      _ = UpdateNoteEntity();
      _ = NoteService.CommitSearchIndexAsync();
      UnregisterMessengers();
    }

    base.Dispose(disposing);
  }

  private void UnregisterEventHandlers()
  {
    _notePropertyDebounceTimer.Elapsed -= NotePropertyChangedDebounceTimer_Elapsed;
    Note.PropertyChanged -= Note_PropertyChanged;
  }
  #endregion

  #region Note 내부 속성 변경 시 데이터베이스에 반영 및 기타 로직 실행
  private readonly HashSet<string> _changedNoteProperties = new();

  private static readonly double _notePropertyDebounceTimerInterval = 500;
  private readonly System.Timers.Timer _notePropertyDebounceTimer = new() { Interval = _notePropertyDebounceTimerInterval, AutoReset = false };

  private async void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    _notePropertyDebounceTimer.Start();

    // 뷰에 반영(TwoWay 바인딩 시) 
    switch (e.PropertyName)
    {
      case nameof(Note.Title):
        await JumpListService.EditJumpListItemAsync(Note);
        break;
      case nameof(Note.BackdropKind):
        ChangeNoteBackdrop();
        if (Note.BackdropKind is not BackdropKind.None)
        {
          Note.ShowBackgroundImage = false;
        }
        break;
      case nameof(Note.BackdropKind) or nameof(Note.BackdropTintOpacity) or nameof(Note.BackdropLuminosityOpacity):
        ChangeNoteBackdropProperties();
        break;
      case nameof(Note.BackgroundColor):
        SelectedPaletteBackgroundColor = PaletteBackgroundColors.FirstOrDefault(b => b.Color == Note.BackgroundColor);
        ChangeNoteBackdropProperties();
        break;
      case nameof(Note.IsBookmarked):
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(Note, nameof(Note.IsBookmarked), !Note.IsBookmarked, Note.IsBookmarked), AppMessageTokens.ChangeNoteIsBookmarkedStateToken);
        break;
      case nameof(Note.IsDeleted):
        break;
      case nameof(Note.ShowBackgroundImage) or nameof(Note.BackgroundImagePath):
        BackgroundImage = Note.ShowBackgroundImage ? GetBackgroundImage(Note.BackgroundImagePath) : null;
        if (Note.ShowBackgroundImage)
        {
          Note.BackdropKind = BackdropKind.None;
        }
        break;
      case nameof(Note.IsAlwaysOnTop):
        NoteWindowService.TryExecuteOnNoteWindow(Note.Id, noteWindow =>
        {
          (noteWindow.AppWindow.Presenter as OverlappedPresenter)?.IsAlwaysOnTop = Note.IsAlwaysOnTop;
        });
        break;
    }

    if (!string.IsNullOrEmpty(e.PropertyName))
    {
      _changedNoteProperties.Add(e.PropertyName);
    }
  }

  private async void NotePropertyChangedDebounceTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) => await UpdateNoteEntity();

  public async Task UpdateNoteEntity()
  {
    await NoteService.UpdateNoteEntityAsync(Note, _changedNoteProperties);
    _changedNoteProperties.Clear();
  }
  #endregion

  public Task<bool> DeleteNoteEntity() => NoteService.DeleteNotePermanentlyAsync(Note.Id);

  private readonly RichEditBox _previewRichEditBox = new();
  private string GetPreview(string body, int start, int end)
  {
    var document = _previewRichEditBox.Document;
    document.SetText(TextSetOptions.FormatRtf, body);
    document.Selection.SetRange(start, end);
    document.Selection.GetText(TextGetOptions.FormatRtf, out var preview);
    return preview;
  }

  public void SetPreview()
  {
    Preview = GetPreview(Note.Body, 0, PreviewTextMaxLength);
  }

  [ObservableProperty]
  public partial string Preview { get; set; }

  [ObservableProperty]
  public partial int PreviewTextMaxLength { get; set; } = 100;

  #region Backdrop and Background Color
  public IReadOnlyList<BackdropKind> BackdropKinds { get; } = Enum.GetValues<BackdropKind>();

  public void ChangeNoteBackdrop()
  {
    NoteWindowService.TryExecuteOnNoteWindow(Note.Id, (noteWindow) =>
    {
      noteWindow.SystemBackdrop = Note.BackdropKind switch
      {
        BackdropKind.Acrylic => new ExtendedAcrylicBackdrop()
        {
          TintColor = Note.BackgroundColor,
          TintOpacity = Note.BackdropTintOpacity,
          LuminosityOpacity = Note.BackdropLuminosityOpacity,
          FallbackColor = GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity)
        },
        BackdropKind.Mica => new ExtendedMicaBackdrop()
        {
          TintColor = Note.BackgroundColor,
          TintOpacity = Note.BackdropTintOpacity,
          LuminosityOpacity = Note.BackdropLuminosityOpacity,
          FallbackColor = GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity)
        },
        BackdropKind.None or _ => null
      };
    });
  }

  private void ChangeNoteBackdropProperties()
  {
    NoteWindowService.TryExecuteOnNoteWindow(Note.Id, (noteWindow) =>
    {
      if (noteWindow.SystemBackdrop is ExtendedSystemBackdrop backdrop)
      {
        backdrop.TintColor = Note.BackgroundColor;
        backdrop.TintOpacity = Note.BackdropTintOpacity;
        backdrop.LuminosityOpacity = Note.BackdropLuminosityOpacity;
        backdrop.FallbackColor = GetFallbackColor(Note.BackgroundColor, Note.BackdropTintOpacity);
      }
    });
  }

  private Color GetFallbackColor(Color color, double opacity) => Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);

  [ObservableProperty]
  public partial BitmapImage? BackgroundImage { get; set; }

  public IReadOnlyList<SolidColorBrush> PaletteBackgroundColors => AppColors.DefaultPaletteColorBrushes;

  private SolidColorBrush? _selectedPaletteBackgroundColor;
  public SolidColorBrush? SelectedPaletteBackgroundColor
  {
    get => _selectedPaletteBackgroundColor;
    set
    {
      if (SetProperty(ref _selectedPaletteBackgroundColor, value) && value is not null)
      {
        Note.BackgroundColor = value.Color;
      }
    }
  }

  private BitmapImage? GetBackgroundImage(string? imagePath)
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
  #endregion

  #region Images
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
  public Command<NavigationId?> CreateNewNoteCommand => NoteCommandService.CreateNewNoteCommand;
  public Command<NoteModel> ViewListCommand => NoteCommandService.ViewListCommand;

  public Command<NoteModel> RemoveNoteCommand => NoteCommandService.RemoveNoteCommand;

  public Command<NoteModel> AddNoteToJumpListCommand => NoteCommandService.AddNoteToJumpListCommand;

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, MessageToken<NoteId>>(this, AppMessageTokens.UpdateNotePreviewToken(Note.Id), (recipient, message) =>
    {
      SetPreview();
    });
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}