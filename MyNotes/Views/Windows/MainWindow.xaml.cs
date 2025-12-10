using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Common.Messages;
using MyNotes.Models.Navigation;
using MyNotes.Resources;
using MyNotes.Services.Database;
using MyNotes.Services.Settings;
using MyNotes.ViewModels;
using MyNotes.Views.Dialogs;

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

    // 뒤로가기 활성화에 따른 드래그 영역 조정
    BackButtonVisibilityPropertyChangedToken = MainWindow_BackButton.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, (obj, dp) =>
    {
      var button = (Button)obj;
      button.LayoutUpdated += MainWindow_BackButton_LayoutUpdated;
    });

    // 창 활성화 변경 시
    this.Activated += MainWindow_Activated;
    this.Closed += MainWindow_Closed;
    AppWindow.Destroying += AppWindow_Destroying;

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

  private void AppWindow_Destroying(AppWindow sender, object args)
  {
    // 창 크기 저장
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);
    SettingsService.Save(SettingsDescriptors.MainWindowSize.Key, new Size(AppWindow.ClientSize.Width / scaleFactor, AppWindow.ClientSize.Height / scaleFactor));

    // 창 위치 및 디스플레이 저장
    SettingsService.Save(SettingsDescriptors.MainWindowPosition.Key, new Point(AppWindow.Position.X, AppWindow.Position.Y));
    SettingsService.Save(SettingsDescriptors.MainWindowDisplay.Key, NativeMethods.GetMonitorInfoForWindow(_hWnd)?.szDevice ?? string.Empty);

    // CanGoBackProperty에 등록한 콜백 해제
    MainWindow_BackButton.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, BackButtonVisibilityPropertyChangedToken);
  }

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    this.Activated -= MainWindow_Activated;

    // 메신저 해제
    UnregisterMessengers();

    // 바인딩 해제
    Bindings.StopTracking();

    // 뷰모델 해제
    ViewModel.Dispose();
  }

  #region 타이틀바 드래그 영역 조정
  private void MainWindow_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void MainWindow_TitleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private readonly long BackButtonVisibilityPropertyChangedToken;
  private void MainWindow_BackButton_LayoutUpdated(object? sender, object e)
  {
    (sender as Button)?.LayoutUpdated -= MainWindow_BackButton_LayoutUpdated;
    SetRegionsForCustomTitleBar();
  }

  private readonly InputNonClientPointerSource _inputNonClientPointerSource;
  private void SetRegionsForCustomTitleBar()
  {
    if (AppWindow is not null && MainWindow_TitleBarGrid.XamlRoot is XamlRoot xamlRoot)
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
  #endregion

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
      ViewModel.CurrentNavigation = _navigationBackStack.Pop();
      _preventNavigation = false;
    }
  }

  private void MainWindow_PaneToggleButton_Click(object sender, RoutedEventArgs e)
  {
    MainWindow_NavigationView.IsPaneOpen = !MainWindow_NavigationView.IsPaneOpen;
  }

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
        ViewModel.AddListCommand?.RaiseCanExecuteChanged();
        if (ViewModel.CurrentNavigation is not null)
          _navigationBackStack.Push(ViewModel.CurrentNavigation);
        ViewModel.CurrentNavigation = coreNode;
        break;
      case NavigationUserNode userNode:
        MainWindow_NavigationFrame.Navigate(userNode.PageType);
        ViewModel.AddListCommand?.RaiseCanExecuteChanged();
        if (ViewModel.CurrentNavigation is not null)
          _navigationBackStack.Push(ViewModel.CurrentNavigation);
        ViewModel.CurrentNavigation = userNode;
        (MainWindow_NavigationView.ContainerFromMenuItem(userNode) as NavigationViewItem)?.IsSelected = true;
        break;
    }
  }

  private void NavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    e.AcceptedOperation = DataPackageOperation.Move;
  }

  private async void NavigationViewItem_Drop(object sender, DragEventArgs e)
  {
    Debug.WriteLine(string.Join(", ", e.DataView.AvailableFormats));
    if (await e.DataView.GetDataAsync($"{App.PackageFamilyName}.NavigationUserNode.Id") is string id)
    {
      if (MainWindow_NavigationView.MenuItemFromContainer(sender as UIElement) is NavigationUserNode node)
      {
        if (NavigationUserNode.FindUserNode(n => n.Id.Value == Guid.Parse(id)) is NavigationUserNode sourceNode &&
          NavigationUserNode.FindUserNode(n => n.Id == node.Id) is NavigationUserNode targetNode)
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

  private void TreeViewItem_Drop(object sender, DragEventArgs e)
  {
    e.Handled = true;
  }

  private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    SetUserNodeDialog dialog = new() { XamlRoot = this.Content.XamlRoot };
    _ = await dialog.ShowAsync();
  }

  private Uri GetIconUri(string icon)
  {
    return new Uri($"ms-appx:///Assets/Icons/FluentEmoji/{icon}");
  }
}

public sealed partial class MainWindow : Window
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, string>(this, MessageTokens.ChangeAppTheme, new((recipient, message) => SetAppTheme(message.Value)));
    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NavigationUserNode, bool>, string>(this, MessageTokens.ChangeUserNodeFocustState, new((recipient, message) =>
    {
      var container = MainWindow_NavigationView.ContainerFromMenuItem(message.Request);
      Console.WriteLine("container type: " + container?.GetType());
      var textbox = container?.FindDescendant(typeof(Grid))?.FindDescendant(typeof(TextBox)) as TextBox;
      Console.WriteLine("first type: " + container?.FindDescendant(typeof(Grid))?.GetType());
      Console.WriteLine("second type: " + container?.FindDescendant(typeof(Grid))?.FindDescendant(typeof(TextBox))?.GetType());
      if (textbox is not null)
      {
        message.Reply(textbox.Focus(FocusState.Programmatic));
      }
      else
      {
        message.Reply(false);
      }
    }));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}

// DEBUG
public sealed partial class MainWindow : Window
{
  private async void MainWindow_DebugMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    foreach (var entity in await context.NavigationEntities.ToListAsync())
    {
      Console.WriteLine(entity.ToString());
      Console.WriteLine();
    }
  }

  private async void MainWindow_ClearDatabaseMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureDeletedAsync();
    ViewModel.UserRootNavigation.ChildNodes.Clear();
  }

  private async void MainWindow_CreateDatabaseMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    var factory = App.Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureCreatedAsync();
  }

  private void MainWindow_GCMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    GC.Collect();
  }
}