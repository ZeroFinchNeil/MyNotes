using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Debugging;
using MyNotes.Helpers;
using MyNotes.Resources;
using MyNotes.Services.Settings;
using MyNotes.Views.Navigations;

namespace MyNotes.Views.Windows;

internal sealed partial class MainWindow : Window
{
  // ServiceProvider(DI)로 주입받은 뷰모델/서비스 필드
  private readonly SettingsService SettingsService;

  // 창 핸들 및 AppWindow Presenter 필드
  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;

  public MainWindow()
  {
#if DEBUG
    ReferenceTracker.MainWindowReference.Add(this, AppWindow.Id.Value);
#endif
    InitializeComponent();
    SettingsService = App.Instance.Services.GetRequiredService<SettingsService>();

    this.ExtendsContentIntoTitleBar = true;

    // 아이콘 설정
    AppWindow.SetIcon("Assets/AppIcon.ico");

    // DPI 스케일 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    var minimumWindowSize = SettingsDescriptors.MainWindowMinimumSize.DefaultValue;
    _presenter = AppWindow.Presenter as OverlappedPresenter;
    _presenter?.PreferredMinimumWidth = (int)(minimumWindowSize.Width * scaleFactor);
    _presenter?.PreferredMinimumHeight = (int)(minimumWindowSize.Height * scaleFactor);

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
    var windowSize = SettingsService.Load(SettingsDescriptors.MainWindowSize);
    if (windowSize.Width < minimumWindowSize.Width && windowSize.Height < minimumWindowSize.Height)
      windowSize = SettingsDescriptors.MainWindowSize.DefaultValue;

    AppWindow.Resize(new((int)(windowSize.Width * scaleFactor), (int)(windowSize.Height * scaleFactor)));

    // 창 초기 위치 지정
    var windowPosition = SettingsService.Load(SettingsDescriptors.MainWindowPosition);
    List<RectInt32> areas = new();
    foreach (var monitor in NativeMethods.GetActiveMonitorsInfo())
    {
      areas.Add(new()
      {
        X = monitor.rcWork.Left - SettingsDescriptors.WindowBorderMargin.DefaultValue,
        Y = monitor.rcWork.Top - SettingsDescriptors.WindowBorderMargin.DefaultValue,
        Width = monitor.rcWork.Right,
        Height = monitor.rcWork.Bottom,
      });
    }
    PointInt32 position = windowPosition.PointInt32;
    if (ContainsPointInAreas(areas, position))
      AppWindow.Move(position);

    // 제목 표시줄 테마 설정
    var theme = (ElementTheme)SettingsService.Load(SettingsDescriptors.AppTheme);
    AppWindow.TitleBar.PreferredTheme = theme switch
    {
      ElementTheme.Light => TitleBarTheme.Light,
      ElementTheme.Dark => TitleBarTheme.Dark,
      _ => TitleBarTheme.UseDefaultAppMode
    };

    this.Content = new MainPage(this);
  }

  private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
  {
    // 창 크기 저장
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);
    SettingsService.Save(SettingsDescriptors.MainWindowSize.Key, new Size(AppWindow.Size.Width / scaleFactor, AppWindow.Size.Height / scaleFactor));

    // 창 위치 및 디스플레이 저장
    SettingsService.Save(SettingsDescriptors.MainWindowPosition.Key, new Point(AppWindow.Position.X, AppWindow.Position.Y));
    SettingsService.Save(SettingsDescriptors.MainWindowDisplay.Key, NativeMethods.GetMonitorInfoForWindow(_hWnd)?.szDevice ?? string.Empty);
  }

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    this.Activated -= MainWindow_Activated;
    AppWindow.Closing -= AppWindow_Closing;
    this.Closed -= MainWindow_Closed;
  }

  public static bool ContainsPointInAreas(List<RectInt32> areas, PointInt32 point)
  {
    foreach (var rect in areas)
    {
      if (rect.X <= point.X && rect.Y <= point.Y && point.X < rect.Width && point.Y < rect.Height)
        return true;
    }

    return false;
  }

  private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<WindowActivationState>(args.WindowActivationState), MessageTokens.MainWindowActivationChangedToken);
  }
}