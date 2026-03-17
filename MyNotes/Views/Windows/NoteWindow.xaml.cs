using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.AppConstants;
using MyNotes.Common.Interop;
using MyNotes.Debugging;
using MyNotes.Models.Notes;
using MyNotes.Models.UI;
using MyNotes.Services.App;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Notes;

namespace MyNotes.Views.Windows;

internal sealed partial class NoteWindow : Window
{
  private readonly WindowService WindowService;

  private readonly IntPtr _hWnd;
  private readonly NoteId NoteId;

  private readonly TaskCompletionSource LoadTCS = new();
  public Task LoadTask => LoadTCS.Task;
  public event EventHandler? Loaded;

  #region Object Lifetime Management
  public NoteWindow(Note note)
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.WindowReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif
    InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;

    var provider = App.Services.GetRequiredService<NoteViewModelProvider>();
    WindowService = App.Services.GetRequiredService<WindowService>();

    // WindowService에 등록
    NoteId = note.Id;
    WindowService.NoteWindows[NoteId] = new WeakReference<NoteWindow>(this);

    // hWnd(Window Handle) 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

    // DPI 스케일 가져오기
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    var minimumWindowSize = AppSettingsDescriptors.NoteWindowMinimumSize.DefaultValue;
    var presenter = AppWindow.Presenter as OverlappedPresenter;
    presenter?.PreferredMinimumWidth = (int)(minimumWindowSize.Width * scaleFactor);
    presenter?.PreferredMinimumHeight = (int)(minimumWindowSize.Height * scaleFactor);
    presenter?.SetBorderAndTitleBar(true, false);

    //AppWindow.Resize(new((int)(note.Size.Width * scaleFactor), (int)(note.Size.Height * scaleFactor)));
    AppWindow.MoveAndResize(new(note.Position.X, note.Position.Y, (int)(note.Size.Width * scaleFactor), (int)(note.Size.Height * scaleFactor)));

    // 창 활성화 변경 시
    this.Activated += NoteWindow_Activated;
    this.Closed += NoteWindow_Closed;

    this.Content = new NotePage(this, note);

    Loaded?.Invoke(this, EventArgs.Empty);
    LoadTCS.TrySetResult();
  }

  public bool IsClosed { get; set; } = false;

  private void NoteWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;

    this.Activated -= NoteWindow_Activated;
    this.Closed -= NoteWindow_Closed;

    // WindowService에서 Window 테이블에서 제거
    WindowService.NoteWindows.Remove(NoteId);
  }
  #endregion

  
  private void NoteWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    
    if (AppWindow.Presenter is OverlappedPresenter presenter)
    {
      WindowPresenterState state = new() { WindowActivationState = args.WindowActivationState, OverlappedPresenterState = presenter.State };
      WeakReferenceMessenger.Default.Send(new ValueChangedMessage<WindowPresenterState>(state), AppMessageTokens.NoteWindowActivationChangedToken(NoteId));
    }
  }
}