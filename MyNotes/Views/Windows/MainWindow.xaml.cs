using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.UI;
using MyNotes.Resources;
using MyNotes.Services.Database;
using MyNotes.Services.Dialog;
using MyNotes.Services.Settings;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;

using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Views.Windows;

internal sealed partial class MainWindow : Window
{
  // ServiceProvider(DI)로 주입받은 뷰모델/서비스 필드
  private readonly MainViewModel ViewModel;
  private readonly SettingsService SettingsService;
  private readonly DialogService DialogService;

  // 창 핸들 및 AppWindow Presenter 필드
  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;

  public MainWindow()
  {
    InitializeComponent();
    ViewModel = App.Instance.Services.GetRequiredService<MainViewModel>();
    SettingsService = App.Instance.Services.GetRequiredService<SettingsService>();
    DialogService = App.Instance.Services.GetRequiredService<DialogService>();

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
    AppWindow.Destroying += AppWindow_Destroying;

    // 창 초기 크기 지정
    var windowSize = SettingsService.Load<Size>(SettingsDescriptors.MainWindowSize.Key);
    if (windowSize.Width < minimumWindowSize.Width && windowSize.Height < minimumWindowSize.Height)
      windowSize = SettingsDescriptors.MainWindowSize.DefaultValue;

    AppWindow.Resize(new((int)(windowSize.Width * scaleFactor), (int)(windowSize.Height * scaleFactor)));

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
    PointInt32 position = windowPosition.PointInt32;
    if (ContainsPointInAreas(areas, position))
      AppWindow.Move(position);

    // 앱 테마 설정
    SetAppTheme((ElementTheme)SettingsService.Load<int>(SettingsDescriptors.AppTheme.Key));

    // 메신저 등록
    RegisterMessengers();

    // 드래그 UI 타이머 등록
    SetDraggableNavigationTimer();
  }

  private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
  {

  }

  private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
  {
    // 창 크기 저장
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);
    SettingsService.Save(SettingsDescriptors.MainWindowSize.Key, new Size(AppWindow.Size.Width / scaleFactor, AppWindow.Size.Height / scaleFactor));

    // 창 위치 및 디스플레이 저장
    SettingsService.Save(SettingsDescriptors.MainWindowPosition.Key, new Point(AppWindow.Position.X, AppWindow.Position.Y));
    SettingsService.Save(SettingsDescriptors.MainWindowDisplay.Key, NativeMethods.GetMonitorInfoForWindow(_hWnd)?.szDevice ?? string.Empty);

    // CanGoBackProperty에 등록한 콜백 해제
    MainWindow_BackButton.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, BackButtonVisibilityPropertyChangedToken);

    // 타이머 해제
    ReleaseDraggableNavigationTimer();
  }

  private void AppWindow_Destroying(AppWindow sender, object args)
  {
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

      RectInt32 BackButtonRect = BackButtonPosition.ToScaledRectInt32(scaleFactor);
      RectInt32 PaneToggleButtonRect = PaneToggleButtonPosition.ToScaledRectInt32(scaleFactor);
      RectInt32 SearchBoxRect = SearchBoxPosition.ToScaledRectInt32(scaleFactor);

      // 제목 표시줄 드래그 제외할 영역 설정
      _inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, [BackButtonRect, PaneToggleButtonRect, SearchBoxRect]);
    }
  }
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
    if (MainWindow_NavigationFrame.CanGoBack)
    {
      _preventNavigation = true;
      MainWindow_NavigationFrame.GoBack();
      ViewModel.PopNavigationBackStack();
      _preventNavigation = false;
    }
  }

  private void MainWindow_PaneToggleButton_Click(object sender, RoutedEventArgs e)
  {
    MainWindow_NavigationView.IsPaneOpen = !MainWindow_NavigationView.IsPaneOpen;
  }

  private bool _preventNavigation = false;

  private void MainWindow_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (_preventNavigation)
      return;

    if (args.SelectedItem is NavigationViewModelBase { Navigation: INavigationNode navigation })
    {
      MainWindow_NavigationFrame.Navigate(navigation.PageType);
      ViewModel.AddListCommand?.RaiseCanExecuteChanged();
      ViewModel.AddGroupCommand?.RaiseCanExecuteChanged();
      ViewModel.PushNavigationBackStack(navigation);
    }
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


  private NavigationViewModelBase? _sourceNavigationViewModel;
  private void DraggableNavigationViewItem_DragStarting(UIElement sender, DragStartingEventArgs args)
  {
    if (sender is FrameworkElement { DataContext: NavigationViewModelBase { Navigation: NavigationUserNode sourceNavigation } sourceViewModel })
    {
      _sourceNavigationViewModel = sourceViewModel;
      args.Data.SetData($"{App.PackageFamilyName}.NavigationUserNode.Id", sourceNavigation.Id.Value.ToString());
    }
  }

  private DispatcherTimer _dispatcherTimer = new() { Interval = TimeSpan.FromMilliseconds(1500) };
  private void SetDraggableNavigationTimer()
  {
    _dispatcherTimer.Tick += DraggableUIDispatcherTimer_Tick;
  }

  private void ReleaseDraggableNavigationTimer()
  {
    _dispatcherTimer.Stop();
    _dispatcherTimer.Tick -= DraggableUIDispatcherTimer_Tick;
  }

  private void DraggableUIDispatcherTimer_Tick(object? sender, object e)
  {
    _dispatcherTimer.Stop();
    _exapndableNavigation?.IsExpanded = !_exapndableNavigation.IsExpanded;
    _exapndableNavigation = null;
  }

  private readonly string _navigationFormatId = $"{App.PackageFamilyName}.NavigationUserNode.Id";
  private DragUISession? _dragUISession;
  private NavigationUserCompositeNode? _exapndableNavigation;

  private async void DraggableNavigationViewItem_DragEnter(object sender, DragEventArgs e)
  {
    e.Handled = true;
    if (await e.DataView.GetDataAsync(_navigationFormatId) is string id)
    {
      if (sender is FrameworkElement { DataContext: NavigationViewModelBase { Navigation: NavigationUserNode navigation } })
      {
        _dragUISession = new()
        {
          FormatId = _navigationFormatId,
          DataView = id,
          DataPackageOperation = DataPackageOperation.Move,
          DragUIOverrideCaption = navigation is NavigationUserLeafNode ? "Move to this position" : "Move as a child of this item"
        };

        _dispatcherTimer.Stop();
        if (navigation is NavigationUserCompositeNode compositeNode)
        {
          _exapndableNavigation = compositeNode;
          _dispatcherTimer.Start();
        }
      }
    }
  }

  private void DraggableNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    e.Handled = true;

    if (_dragUISession is DragUISession dragUISession && !dragUISession.IsExpired)
    {
      e.AcceptedOperation = dragUISession.DataPackageOperation;
      e.DragUIOverride.Caption = dragUISession.DragUIOverrideCaption;
      dragUISession.Dispose();
    }
  }

  private async void DraggableNavigationViewItem_Drop(object sender, DragEventArgs e)
  {
    e.Handled = true;
    _dispatcherTimer.Stop();

    if (sender is FrameworkElement { DataContext: NavigationViewModelBase { Navigation: NavigationUserNode targetNavigation } } && _sourceNavigationViewModel is NavigationViewModelBase { Navigation: NavigationUserNode sourceNavigation })
    {
      if (sourceNavigation == targetNavigation)
        return;
      var sourceParentNavigation = sourceNavigation.Parent;
      var targetParentNavigation = targetNavigation.Parent;
      int targetIndex = targetParentNavigation.ChildNodes.IndexOf(targetNavigation);

      if (sourceParentNavigation == targetParentNavigation)
      {
        int sourceIndex = sourceParentNavigation.ChildNodes.IndexOf(sourceNavigation);
        targetParentNavigation.ChildNodes.Move(sourceIndex, targetIndex);
      }
      else
      {
        sourceParentNavigation.ChildNodes.Remove(sourceNavigation);
        targetParentNavigation.ChildNodes.Insert(targetIndex, sourceNavigation);
      }
      //switch (targetNavigation)
      //{
      //  case NavigationUserLeafNode targetLeafNavigation:
      //    var targetParentNavigation = targetLeafNavigation.Parent;
      //    int targetIndex = targetParentNavigation.ChildNodes.IndexOf(targetNavigation);
      //    if (sourceParentNavigation == targetParentNavigation)
      //    {
      //      int sourceIndex = sourceParentNavigation.ChildNodes.IndexOf(sourceNavigation);
      //      targetParentNavigation.ChildNodes.Move(sourceIndex, targetIndex);
      //    }
      //    else
      //    {
      //      sourceParentNavigation.ChildNodes.Remove(sourceNavigation);
      //      targetParentNavigation.ChildNodes.Insert(targetIndex, sourceNavigation);
      //    }
      //    break;
      //  case NavigationUserCompositeNode targetCompositeNavigation:
      //    if (sourceParentNavigation != targetCompositeNavigation)
      //    {
      //      sourceParentNavigation.ChildNodes.Remove(sourceNavigation);
      //      targetCompositeNavigation.ChildNodes.Insert(targetCompositeNavigation.ChildNodes.Count, sourceNavigation);
      //    }
      //    break;
      //}
    }
  }

  private void CommandBarFlyout_Opening(object sender, object e)
  {
    if (sender is MenuFlyout flyout
      && flyout.GetValue(DataContextHelper.DataContextProperty) is UserNavigationViewModel currentVM)
    {
      NavigationUserNode? currentGroup = currentVM switch
      {
        UserLeafNavigationViewModel leaf => leaf.Navigation.Parent,
        UserCompositeNavigationViewModel composite => composite.Navigation,
        _ => null
      };

      flyout.Items.Clear();
      foreach (var targetVM in ViewModel.GroupNavigationViewModels)
      {
        if (targetVM.Navigation == currentGroup)
          continue;
        flyout.Items.Add(new MenuFlyoutItem
        {
          Text = targetVM.Navigation.Title,
          Icon = new ImageIcon() { Source = targetVM.Navigation.IconImage },
          Command = currentVM.MoveToGroupCommand,
          CommandParameter = (currentVM as NavigationViewModelBase, targetVM as NavigationViewModelBase)
        });
      }
    }
  }
}

internal sealed partial class MainWindow : Window
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

// DEBUG
internal sealed partial class MainWindow : Window
{
  private async void MainWindow_SeparatorMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    Console.WriteLine();
    Console.WriteLine("--------------------");
    Console.WriteLine();
  }

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
    ViewModel.UserRootNavigationViewModel?.Navigation.ChildNodes.Clear();
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

