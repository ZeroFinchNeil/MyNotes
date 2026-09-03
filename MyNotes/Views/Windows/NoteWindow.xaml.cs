using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Constants;
using MyNotes.Domain.Notes;
using MyNotes.Messaging;
using MyNotes.Messaging.Messages;
using MyNotes.Models.Notes;
using MyNotes.Models.UI;
using MyNotes.Services.Windows;
using MyNotes.ViewModels.Notes.Providers;
using MyNotes.Views.Notes;

namespace MyNotes.Views.Windows;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NoteWindow : Window
{
  private readonly NoteWindowService NoteWindowService;

  private readonly IntPtr _hWnd;
  private readonly NoteId NoteId;
  public Task InitializationTask { get; }
  public event EventHandler? Loaded;

  private NotePage? _content;

  #region Object Lifetime Management
  public NoteWindow(NoteModel note)
  {
    TrackReference();
    InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;

    // 아이콘 설정
    AppWindow.SetIcon(AppStrings.AppIconPath);
    AppWindow.SetTaskbarIcon(AppStrings.AppIconPath);

    var provider = App.Services.GetRequiredService<NoteViewModelProvider>();
    NoteWindowService = App.Services.GetRequiredService<NoteWindowService>();

    // WindowService에 등록
    NoteId = note.Id;
    NoteWindowService.NoteWindowTable[NoteId] = new WeakReference<NoteWindow>(this);

    InitializationTask = InitializeAsync(note);

    // hWnd(Window Handle) 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

    // DPI 스케일 가져오기
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    var minimumWindowSize = AppSettingsDescriptors.NoteWindowMinimumSize;
    var presenter = AppWindow.Presenter as OverlappedPresenter;
    presenter?.PreferredMinimumWidth = (int)(minimumWindowSize.Width * scaleFactor);
    presenter?.PreferredMinimumHeight = (int)(minimumWindowSize.Height * scaleFactor);
    presenter?.SetBorderAndTitleBar(true, false);

    //AppWindow.Resize(new((int)(note.Size.Width * scaleFactor), (int)(note.Size.Height * scaleFactor)));
    AppWindow.MoveAndResize(new(note.Position.X, note.Position.Y, (int)(note.Size.Width * scaleFactor), (int)(note.Size.Height * scaleFactor)));

    // 창 활성화 변경 시
    this.Activated += NoteWindow_Activated;
    this.Closed += NoteWindow_Closed;
  }

  private async Task InitializeAsync(NoteModel noteModel)
  {
    _content = new NotePage(noteModel);
    await _content.InitializationTask;
    this.Content = _content;
    this.SetTitleBar(_content.TitleBarElement);

    Loaded?.Invoke(this, EventArgs.Empty);
  }

  public bool IsClosed { get; set; } = false;

  private async void NoteWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;

    this.Activated -= NoteWindow_Activated;
    this.Closed -= NoteWindow_Closed;

    // WindowService에서 Window 테이블에서 제거
    NoteWindowService.NoteWindowTable.Remove(NoteId);

    if (_content is not null)
    {
      await _content.DisposeAsync();
      _content = null;
    }
  }
  #endregion

  private void NoteWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    if (AppWindow.Presenter is OverlappedPresenter presenter)
    {
      WindowPresenterState state = new() { WindowActivationState = args.WindowActivationState, OverlappedPresenterState = presenter.State };
      WeakReferenceMessenger.Default.Send(new NoteWindowActivationChangedMessage(state), MessageToken<NoteId>.Create(NoteId));
    }
  }
}