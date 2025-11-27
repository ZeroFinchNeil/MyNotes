using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Models;
using MyNotes.Services.Settings;
using MyNotes.ViewModels;

using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Views.Windows;

public sealed partial class MainWindow : Window
{
  // ServiceProvider(DI)로 주입받은 뷰모델/서비스 필드
  private readonly MainViewModel ViewModel;
  private readonly SettingsService SettingsService;

  // 창 핸들 및 AppWindow Presenter 필드
  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;

  public MainWindow()
  {
    InitializeComponent();
    ViewModel = App.Instance.Services.GetRequiredService<MainViewModel>();
    SettingsService = App.Instance.Services.GetRequiredService<SettingsService>();

    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(MainWindow_TitleBarGrid);

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

    // 타이틀 바에 캡션 컨트롤 여백 및 드래그 제외 영역 지정
    _inputNonClientPointerSource = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
    MainWindow_TitleBarGrid.Loaded += MainWindow_TitleBarGrid_Loaded;
    MainWindow_TitleBarGrid.SizeChanged += MainWindow_TitleBarGrid_SizeChanged;

    // 창 활성화 변경 시
    this.Activated += MainWindow_Activated;
    this.Closed += MainWindow_Closed;

    // 창 초기 크기 지정
    var windowSize = SettingsService.Load<Size>(SettingsDescriptors.MainWindowSize.Key);
    if (windowSize.Width < minimumWindowSize.Width && windowSize.Height < minimumWindowSize.Height)
      windowSize = SettingsDescriptors.MainWindowSize.DefaultValue;

    AppWindow.ResizeClient(new((int)(windowSize.Width * scaleFactor), (int)(windowSize.Height * scaleFactor)));

    // 창 초기 위치 지정
    var windowPosition = SettingsService.Load<Point>(SettingsDescriptors.MainWindowPosition.Key);
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
    PointInt32 position = new((int)windowPosition.X, (int)windowPosition.Y);
    if (ContainsPointInAreas(areas, position))
      AppWindow.Move(new(position.X, position.Y));

    // 앱 테마 설정
    SetAppTheme((ElementTheme)SettingsService.Load<int>(SettingsDescriptors.AppTheme.Key));

    // 메신저 등록
    RegisterMessengers();
  }

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    this.Activated -= MainWindow_Activated;

    // 창 크기 저장
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);
    SettingsService.Save(SettingsDescriptors.MainWindowSize.Key, new Size(AppWindow.ClientSize.Width / scaleFactor, AppWindow.ClientSize.Height / scaleFactor));

    // 창 위치 및 디스플레이 저장
    SettingsService.Save(SettingsDescriptors.MainWindowPosition.Key, new Point(AppWindow.Position.X, AppWindow.Position.Y));
    SettingsService.Save(SettingsDescriptors.MainWindowDisplay.Key, NativeMethods.GetMonitorInfoForWindow(_hWnd)?.szDevice ?? string.Empty);

    // 메신저 해제
    UnregisterMessengers();

    // 바인딩 해제
    Bindings.StopTracking();
  }

  private void MainWindow_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void MainWindow_TitleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private readonly InputNonClientPointerSource _inputNonClientPointerSource;
  private void SetRegionsForCustomTitleBar()
  {
    if (MainWindow_TitleBarGrid.XamlRoot is XamlRoot xamlRoot)
    {
      double scaleFactor = xamlRoot.RasterizationScale;

      // FlowDirection에 따른 캡션 컨트롤 여백 지정
      RightPaddingColumn.Width = new GridLength(Math.Max(0, AppWindow.TitleBar.RightInset) / scaleFactor);
      LeftPaddingColumn.Width = new GridLength(Math.Max(0, AppWindow.TitleBar.LeftInset) / scaleFactor);

      // 뒤로 가기 버튼, 메뉴 버튼, 검색 상자 영역 위치와 크기 계산
      var BackButtonPosition = MainWindow_BackButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_BackButton.ActualWidth, MainWindow_BackButton.ActualHeight));
      var PaneToggleButtonPosition = MainWindow_PaneToggleButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_PaneToggleButton.ActualWidth, MainWindow_PaneToggleButton.ActualHeight));
      var SearchBoxPosition = MainWindow_SearchAutoSuggestBox.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_SearchAutoSuggestBox.ActualWidth, MainWindow_SearchAutoSuggestBox.ActualHeight));

      RectInt32 BackButtonRect = GetRect(BackButtonPosition, scaleFactor);
      RectInt32 PaneToggleButtonRect = GetRect(PaneToggleButtonPosition, scaleFactor);
      RectInt32 SearchBoxRect = GetRect(SearchBoxPosition, scaleFactor);

      // 제목 표시줄 드래그 제외할 영역 설정
      _inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, [BackButtonRect, PaneToggleButtonRect, SearchBoxRect]);
    }
  }

  private static RectInt32 GetRect(Rect bounds, double scale) =>
    new(
      _X: (int)Math.Round(bounds.X * scale),
      _Y: (int)Math.Round(bounds.Y * scale),
      _Width: (int)Math.Round(bounds.Width * scale),
      _Height: (int)Math.Round(bounds.Height * scale)
    );

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
    if (args.WindowActivationState == WindowActivationState.Deactivated)
    {
      _inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, null);
      VisualStateManager.GoToState(MainWindow_RootControl, "WindowDeactivated", false);
    }
    else
    {
      SetRegionsForCustomTitleBar();
      VisualStateManager.GoToState(MainWindow_RootControl, "WindowActivated", false);
    }
  }

  private void MainWindow_BackButton_Click(object sender, RoutedEventArgs e)
  {
    if (MainWindow_NavigationFrame.CanGoBack && _navigationBackStack.Count > 0)
    {
      _preventNavigation = true;
      MainWindow_NavigationFrame.GoBack();
      MainWindow_NavigationView.SelectedItem = _navigationBackStack.Pop();
      _preventNavigation = false;
    }
  }

  private void MainWindow_PaneToggleButton_Click(object sender, RoutedEventArgs e)
  {
    MainWindow_NavigationView.IsPaneOpen = !MainWindow_NavigationView.IsPaneOpen;
  }

  private INavigation? _currentNavigation;
  private readonly Stack<INavigation> _navigationBackStack = new();

  private bool _preventNavigation = false;

  private void MainWindow_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (_preventNavigation)
      return;

    switch (args.SelectedItem)
    {
      case NavigationCoreNode coreNode:
        MainWindow_NavigationFrame.Navigate(coreNode.PageType);
        if (_currentNavigation is not null)
          _navigationBackStack.Push(_currentNavigation);
        _currentNavigation = coreNode;
        break;
      case NavigationUserNode userNode:
        MainWindow_NavigationFrame.Navigate(userNode.PageType);
        if (_currentNavigation is not null)
          _navigationBackStack.Push(_currentNavigation);
        _currentNavigation = userNode;
        break;
    }
  }

  private void ReorderableNavigationViewItem_DragStarting(UIElement sender, DragStartingEventArgs args)
  {
    if (MainWindow_NavigationView.MenuItemFromContainer(sender) is NavigationUserNode node)
    {
      args.Data.SetData($"{App.PackageFamilyName}.NavigationUserNode.Id", node.Id.ToString());
    }
  }

  private void ReorderableNavigationViewItem_DropCompleted(UIElement sender, DropCompletedEventArgs args)
  {
    //Debug.WriteLine("DropCompleted");
  }

  private void ReorderableNavigationViewItem_DragEnter(object sender, DragEventArgs e)
  {
    //Debug.WriteLine("DragEnter");
  }

  private void ReorderableNavigationViewItem_DragLeave(object sender, DragEventArgs e)
  {
    //Debug.WriteLine("DragLeave");
  }

  private void ReorderableNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    e.AcceptedOperation = DataPackageOperation.Move;
  }

  private async void ReorderableNavigationViewItem_Drop(object sender, DragEventArgs e)
  {
    Debug.WriteLine(string.Join(", ", e.DataView.AvailableFormats));
    if (await e.DataView.GetDataAsync($"{App.PackageFamilyName}.NavigationUserNode.Id") is string id)
    {
      if (MainWindow_NavigationView.MenuItemFromContainer(sender as UIElement) is NavigationUserNode node)
      {
        if (ViewModel.GetUserNode(n => n.Id.Value == Guid.Parse(id)) is NavigationUserNode sourceNode &&
          ViewModel.GetUserNode(n => n.Id == node.Id) is NavigationUserNode targetNode)
        {
        }
      }
    }

    e.Handled = true;
  }

  private void MainWindow_NavigationView_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    MainWindow_MoveListsInnerGrid.Height = e.NewSize.Height;
  }

  private void MainWindow_PaneFooter_MoveListsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(MainWindow_RootControl, "MoveListsPopupVisible", false);
  }

  private void MainWindow_MoveListsApplyButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(MainWindow_RootControl, "MoveListsPopupCollapsed", false);
  }

  private void MainWindow_DebugMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    //var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
    //Console.WriteLine($"DisplayId: {displayArea.DisplayId.Value}");
    //Console.WriteLine($"IsPrimary: {displayArea.IsPrimary}");
    //Console.WriteLine($"OuterBounds: ({displayArea.OuterBounds.X}, {displayArea.OuterBounds.Y}), {displayArea.OuterBounds.Width} X {displayArea.OuterBounds.Height}");
    //Console.WriteLine($"WorkArea: ({displayArea.WorkArea.X}, {displayArea.WorkArea.Y}), {displayArea.WorkArea.Width} X{displayArea.WorkArea.Height}");

    _ = NativeMethods.GetWindowRect(_hWnd, out var oRect);
    _ = NativeMethods.GetClientRect(_hWnd, out var cRect);
    Console.WriteLine("----- Window -----");
    Console.WriteLine("{0,-15} ({1,4}, {2,4}) {3,4} X {4,4}", "AppWindow(O):", AppWindow.Position.X, AppWindow.Position.Y, AppWindow.Size.Width, AppWindow.Size.Height);
    Console.WriteLine("{0,-15} ({1,4}, {2,4}) {3,4} X {4,4}", "HWND(O):", oRect.Left, oRect.Top, oRect.Right - oRect.Left, oRect.Bottom - oRect.Top);
    Console.WriteLine("{0,-15} ({1,4}, {2,4}) {3,4} X {4,4}", "AppWindow(C):", AppWindow.Position.X, AppWindow.Position.Y, AppWindow.ClientSize.Width, AppWindow.ClientSize.Height);
    Console.WriteLine("{0,-15} ({1,4}, {2,4}) {3,4} X {4,4}", "HWND(C):", cRect.Left, cRect.Top, cRect.Right - cRect.Left, cRect.Bottom - cRect.Top);
    Console.WriteLine();
  }

  private void SetAppTheme(ElementTheme theme)
  {
    MainWindow_RootControl.RequestedTheme = theme;

    AppWindow.TitleBar.PreferredTheme = theme switch
    {
      ElementTheme.Light => TitleBarTheme.Light,
      ElementTheme.Dark => TitleBarTheme.Dark,
      _ => TitleBarTheme.UseDefaultAppMode
    };
  }
}

public sealed partial class MainWindow : Window
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, string>(this, MessageTokens.ChangeAppTheme, new((recipient, message) => SetAppTheme(message.Value)));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}

public class MainWindowNavigationViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? NavigationCoreNodeTemplate { get; set; }
  public DataTemplate? NavigationSeparatorTemplate { get; set; }
  public DataTemplate? NavigationUserCompositeNodeTemplate { get; set; }
  public DataTemplate? NavigationUserLeafNodeTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      NavigationCoreNode => NavigationCoreNodeTemplate,
      NavigationSeparator => NavigationSeparatorTemplate,
      NavigationUserCompositeNode => NavigationUserCompositeNodeTemplate,
      NavigationUserLeafNode => NavigationUserLeafNodeTemplate,
      _ => null
    };
  }
}

public class MainWindowTreeViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? NavigationUserCompositeNodeTemplate { get; set; }
  public DataTemplate? NavigationUserLeafNodeTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      NavigationUserCompositeNode => NavigationUserCompositeNodeTemplate,
      NavigationUserLeafNode => NavigationUserLeafNodeTemplate,
      _ => null
    };
  }
}