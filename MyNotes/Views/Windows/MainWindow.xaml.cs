using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Helpers;
using MyNotes.Common.Interop;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Settings;
using MyNotes.Shared.Constants;
using MyNotes.Views.Navigations;

namespace MyNotes.Views.Windows;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class MainWindow : Window
{
  // ServiceProvider(DI)로 주입받은 뷰모델/서비스 필드
  private readonly SettingsService SettingsService;

  // 창 핸들 및 AppWindow Presenter 필드
  private readonly IntPtr _hWnd;

  private readonly TaskCompletionSource LoadTCS = new();
  public Task LoadTask => LoadTCS.Task;
  public event EventHandler? Loaded;

  #region Object Lifetime Management
  public MainWindow(NavigationId? _initialNavigationId = null)
  {
    TrackReference();
    InitializeComponent();
    SettingsService = App.Services.GetRequiredService<SettingsService>();

    this.ExtendsContentIntoTitleBar = true;

    // 아이콘 설정
    AppWindow.SetIcon("Assets/AppIcon.ico");

    // DPI 스케일 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    var minimumWindowSize = AppSettingsDescriptors.MainWindowMinimumSize.DefaultValue;
    var presenter = AppWindow.Presenter as OverlappedPresenter;
    presenter?.PreferredMinimumWidth = (int)(minimumWindowSize.Width * scaleFactor);
    presenter?.PreferredMinimumHeight = (int)(minimumWindowSize.Height * scaleFactor);

    // 높은(48epx) 캡션 컨트롤 지원
    AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

    // 창 활성화 및 크기 변경 시
    this.Activated += MainWindow_Activated;
    //AppWindow.Changed += AppWindow_Changed;

    // 창 종료 시 (AppWindow는 hWnd 기준, Window는 XAML 기준)
    // ├─ AppWindow.Closing          // 취소 가능, UI 상태 신뢰 가능
    // ├─ XAML Window.Closed         // 시각 요소/논리 요소 정리
    // └─ Win32 DestroyWindow        // 내부 Win32 창 파괴
    //    └─ AppWindow.Destroying    // 창이 제거된 직후
    AppWindow.Closing += AppWindow_Closing;
    this.Closed += MainWindow_Closed;

    // 창 초기 크기 지정
    var windowSize = SettingsService.Load(AppSettingsDescriptors.MainWindowSize);
    if (windowSize.Width < minimumWindowSize.Width && windowSize.Height < minimumWindowSize.Height)
    {
      windowSize = AppSettingsDescriptors.MainWindowSize.DefaultValue;
    }

    _windowSize = windowSize.SizeInt32;
    AppWindow.Resize(new((int)(_windowSize.Width * scaleFactor), (int)(_windowSize.Height * scaleFactor)));

    // 창 초기 위치 지정
    var windowPosition = SettingsService.Load(AppSettingsDescriptors.MainWindowPosition);
    List<RectInt32> areas = new();
    foreach (var monitor in NativeMethods.GetActiveMonitorsInfo())
    {
      areas.Add(new()
      {
        X = monitor.rcWork.Left - AppSettingsDescriptors.WindowBorderMargin.DefaultValue,
        Y = monitor.rcWork.Top - AppSettingsDescriptors.WindowBorderMargin.DefaultValue,
        Width = monitor.rcWork.Right,
        Height = monitor.rcWork.Bottom,
      });
    }
    _windowPosition = windowPosition.PointInt32;

    if (ContainsPointInAreas(areas, _windowPosition))
    {
      AppWindow.Move(_windowPosition);
    }

    AppWindow.Changed += AppWindow_Changed;

    // 제목 표시줄 테마 설정
    var theme = (ElementTheme)SettingsService.Load(AppSettingsDescriptors.AppTheme);
    AppWindow.TitleBar.PreferredTheme = theme switch
    {
      ElementTheme.Light => TitleBarTheme.Light,
      ElementTheme.Dark => TitleBarTheme.Dark,
      _ => TitleBarTheme.UseDefaultAppMode
    };

    // MainWindow 시작 플래그
    SettingsService.Save(AppSettingsDescriptors.IsMainWindowOpen, true);

    this.Content = new MainPage(this, _initialNavigationId);

    Loaded?.Invoke(this, EventArgs.Empty);
    LoadTCS.TrySetResult();
  }

  private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
  {
    // 창 크기 저장
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);
    SettingsService.Save(AppSettingsDescriptors.MainWindowSize, new Size(_windowSize.Width / scaleFactor, _windowSize.Height / scaleFactor));

    // 창 위치 및 디스플레이 저장
    SettingsService.Save(AppSettingsDescriptors.MainWindowPosition, new Point(_windowPosition.X, _windowPosition.Y));
    SettingsService.Save(AppSettingsDescriptors.MainWindowDisplay, NativeMethods.GetMonitorInfoForWindow(_hWnd)?.szDevice ?? string.Empty);

    // MainWindow 종료 플래그
    SettingsService.Save(AppSettingsDescriptors.IsMainWindowOpen, false);
  }

  public bool IsClosed { get; private set; } = false;

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;
    AppWindow.Changed -= AppWindow_Changed;
    this.Activated -= MainWindow_Activated;
    AppWindow.Closing -= AppWindow_Closing;
    this.Closed -= MainWindow_Closed;
  }
  #endregion

  public void SetNavigation(NavigationId? navigationId) => (this.Content as MainPage)?.SetNavigation(navigationId);

  private SizeInt32 _windowSize;
  private PointInt32 _windowPosition;
  private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
  {
    if (args.DidSizeChange)
    {
      if (AppWindow.Presenter is OverlappedPresenter presenter
        && presenter.State is OverlappedPresenterState.Restored)
      {
        _windowSize = AppWindow.Size;
      }
    }
    else if (args.DidPositionChange)
    {
      if (AppWindow.Presenter is OverlappedPresenter presenter
        && presenter.State is OverlappedPresenterState.Restored)
      {
        _windowPosition = AppWindow.Position;
      }
    }
  }

  public static bool ContainsPointInAreas(List<RectInt32> areas, PointInt32 point)
  {
    foreach (var rect in areas)
    {
      if (rect.X <= point.X && rect.Y <= point.Y && point.X < rect.Width && point.Y < rect.Height)
      {
        return true;
      }
    }

    return false;
  }

  private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<WindowActivationState>(args.WindowActivationState), AppMessageTokens.MainWindowActivationChangedToken);
  }
}