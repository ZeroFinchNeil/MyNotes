using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Content;

using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Debugging;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.UI;
using MyNotes.Services.Logging;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.Views.Windows;

using Windows.ApplicationModel.DataTransfer;

using WinRT.Interop;

namespace MyNotes.Views.Navigations;

internal sealed partial class MainPage : Page
{
  private readonly MainViewModel ViewModel;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;
  private readonly LoggingService LoggingService;
  private readonly IServiceScope ServiceScope;

  private readonly NavigationId _initialNavigationId;

  public MainPage(MainWindow mainWindow, NavigationId? initialNavigationId = null)
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.PageReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif

    InitializeComponent();

    ServiceScope = App.Services.CreateScope();
    ViewModel = ServiceScope.ServiceProvider.GetRequiredService<MainViewModel>();
    SettingsService = ServiceScope.ServiceProvider.GetRequiredService<SettingsService>();
    WindowService = ServiceScope.ServiceProvider.GetRequiredService<WindowService>();
    LoggingService = ServiceScope.ServiceProvider.GetRequiredService<LoggingService>();

    mainWindow.SetTitleBar(MainPage_TitleBarGrid);

    // 시작 내비게이션(페이지) 설정
    _initialNavigationId = initialNavigationId ?? NavigationId.GetOrCreate(SettingsService.Load(SettingsDescriptors.InitialPageId));

    // 앱 테마 설정 
    var theme = (ElementTheme)SettingsService.Load(SettingsDescriptors.AppTheme);
    this.RequestedTheme = theme;

    // 메신저 등록
    RegisterMessengers();

    // 드래그 UI 타이머 등록
    SetDraggableNavigationTimer();

    this.Loaded += MainPage_Loaded;
    this.Unloaded += MainPage_Unloaded;
  }

  private async void MainPage_OpenDebugWindowMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    await App.Instance.OpenDebugWindow();
  }

  private async void MainPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
    ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    await ViewModel.InitializeNavigation();
    SetNavigation(_initialNavigationId);
  }

  public void SetNavigation(NavigationId? navigationId) => ViewModel.SetNavigation(navigationId ?? _initialNavigationId);

  private void MainPage_BackButton_LayoutUpdated(object? sender, object e)
  {
    MainPage_BackButton.LayoutUpdated -= MainPage_BackButton_LayoutUpdated;
    SetRegionsForCustomTitleBar();
  }

  private bool _canGoBack = false;
  private void MainPage_NavigationFrame_Navigated(object sender, NavigationEventArgs e)
  {
    bool canGoBack = MainPage_NavigationFrame.CanGoBack;
    if (_canGoBack != canGoBack)
    {
      _canGoBack = canGoBack;
      MainPage_BackButton.LayoutUpdated += MainPage_BackButton_LayoutUpdated;
    }
  }

  private bool TryGetWindowInfo(out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
  {
    hWnd = IntPtr.Zero;
    appWindow = null;

    try
    {
      if (this.XamlRoot is XamlRoot xamlRoot
        && xamlRoot.ContentIslandEnvironment is ContentIslandEnvironment env)
      {
        var windowId = env.AppWindowId;
        hWnd = Win32Interop.GetWindowFromWindowId(windowId);
        appWindow = AppWindow.GetFromWindowId(windowId);
      }
      else if (WindowService.TryGetCurrentMainWindow(out var mainWindow))
      {
        hWnd = WindowNative.GetWindowHandle(mainWindow);
        appWindow = mainWindow.AppWindow;
      }
    }
    catch (Exception e)
    {
      LoggingService.Write(e);
    }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

  private bool TryExecuteOnWindow(Action<MainWindow> action)
  {
    if (WindowService.TryGetCurrentMainWindow(out var mainWindow))
    {
      action.Invoke(mainWindow);
      return true;
    }
    return false;
  }

  private void MainPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel.UserRootNavigationViewModel.ForEachDescendant((viewmodel) =>
    {
      if (MainPage_NavigationView.ContainerFromMenuItem(viewmodel) is UserNavigationViewItem container)
      {
        container.PresenterDragStarting -= MainPageUserNavigationViewItem_PresenterDragStarting;
        container.DragEnter -= MainPageUserNavigationViewItem_DragEnter;
        container.DragOver -= MainPageUserNavigationViewItem_DragOver;
        container.Drop -= MainPageUserNavigationViewItem_Drop;
      }
    });

    // 이벤트 핸들러 해제
    ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

    // 타이머 해제
    ReleaseDraggableNavigationTimer();

    // 메신저 해제
    UnregisterMessengers();

    // 뷰모델 해제
    ViewModel.Dispose();

    // 서비스 스코프 해제
    ServiceScope.Dispose();

    // 바인딩 해제
    Bindings.StopTracking();
  }

  #region 타이틀바 드래그 영역 조정
  private void MainPage_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void MainPage_TitleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void SetRegionsForCustomTitleBar()
  {
    if (TryGetWindowInfo(out _, out var appWindow) && this.XamlRoot is XamlRoot xamlRoot)
    {
      double scaleFactor = xamlRoot.RasterizationScale;

      // FlowDirection에 따른 캡션 컨트롤 여백 지정
      RightPaddingColumn.Width = new GridLength(Math.Max(0, appWindow.TitleBar.RightInset) / scaleFactor);
      LeftPaddingColumn.Width = new GridLength(Math.Max(0, appWindow.TitleBar.LeftInset) / scaleFactor);

      // 뒤로 가기 버튼, 메뉴 버튼, 검색 상자 영역 위치와 크기 계산
      var BackButtonPosition = MainPage_BackButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainPage_BackButton.ActualWidth, MainPage_BackButton.ActualHeight));
      var PaneToggleButtonPosition = MainPage_PaneToggleButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainPage_PaneToggleButton.ActualWidth, MainPage_PaneToggleButton.ActualHeight));
      var SearchBoxPosition = MainPage_SearchAutoSuggestBox.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainPage_SearchAutoSuggestBox.ActualWidth, MainPage_SearchAutoSuggestBox.ActualHeight));

      RectInt32 BackButtonRect = BackButtonPosition.AsScaledRectInt32(scaleFactor);
      RectInt32 PaneToggleButtonRect = PaneToggleButtonPosition.AsScaledRectInt32(scaleFactor);
      RectInt32 SearchBoxRect = SearchBoxPosition.AsScaledRectInt32(scaleFactor);

      // 제목 표시줄 드래그 제외할 영역 설정
      var _inputNonClientPointerSource = InputNonClientPointerSource.GetForWindowId(appWindow.Id);
      _inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, [BackButtonRect, PaneToggleButtonRect, SearchBoxRect]);
    }
  }
  #endregion

  private void MainPage_BackButton_Click(object sender, RoutedEventArgs e)
  {
    if (MainPage_NavigationFrame.CanGoBack)
    {
      _preventNavigation = true;
      MainPage_NavigationFrame.GoBack();
      ViewModel.PopNavigation();
      _preventNavigation = false;
    }
  }

  private void MainPage_PaneToggleButton_Click(object sender, RoutedEventArgs e)
  {
    MainPage_NavigationView.IsPaneOpen = !MainPage_NavigationView.IsPaneOpen;
  }

  private bool _preventNavigation = false;

  private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(MainViewModel.CurrentNavigationViewModel):
        if (_preventNavigation)
          return;
        if (ViewModel.CurrentNavigationViewModel is NavigationViewModelBase { Navigation: INavigation navigation })
        {
          switch (navigation)
          {
            case NavigationUserCompositeNode:
              return;
            case INavigationNode node:
              MainPage_NavigationFrame.Navigate(node.PageType, navigation);
              break;
            case NavigationSearch search:
              MainPage_NavigationFrame.Navigate(search.PageType, navigation);
              break;
          }

          ViewModel.AddListCommand?.RaiseCanExecuteChanged();
          ViewModel.AddGroupCommand?.RaiseCanExecuteChanged();
          ViewModel.PushNavigation(navigation);
        }
        break;
    }
  }

  private void SetAppTheme(ElementTheme theme)
  {
    this.RequestedTheme = theme;

    if (TryGetWindowInfo(out _, out var appWindow))
    {
      appWindow.TitleBar.PreferredTheme = theme switch
      {
        ElementTheme.Light => TitleBarTheme.Light,
        ElementTheme.Dark => TitleBarTheme.Dark,
        _ => TitleBarTheme.UseDefaultAppMode
      };
    }
  }


  private NavigationViewModelBase? _sourceNavigationViewModel;
  private void MainPageUserNavigationViewItem_PresenterDragStarting(UIElement sender, DragStartingEventArgs args)
  {
    if (sender is UserNavigationViewItem { ViewModel: NavigationViewModelBase { Navigation: NavigationUserNode sourceNavigation } sourceViewModel })
    {
      _sourceNavigationViewModel = sourceViewModel;
      args.Data.SetData(_navigationFormatId, sourceNavigation.Id.Value.ToString());
    }
  }

  private readonly DispatcherTimer _dispatcherTimer = new() { Interval = TimeSpan.FromMilliseconds(1500) };
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

  private async void MainPageUserNavigationViewItem_DragEnter(object sender, DragEventArgs e)
  {
    e.Handled = true;

    if (e.DataView.Contains(_navigationFormatId) && await e.DataView.GetDataAsync(_navigationFormatId) is string id)
    {
      if (sender is UserNavigationViewItem { ViewModel: NavigationViewModelBase { Navigation: NavigationUserNode navigation } })
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

  private void MainPageUserNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    e.Handled = true;

    if (_dragUISession is DragUISession dragUISession && !dragUISession.IsExpired)
    {
      e.AcceptedOperation = dragUISession.DataPackageOperation;
      e.DragUIOverride.Caption = dragUISession.DragUIOverrideCaption;
      dragUISession.Dispose();
    }
  }

  private async void MainPageUserNavigationViewItem_Drop(object sender, DragEventArgs e)
  {
    e.Handled = true;
    _dispatcherTimer.Stop();

    if (sender is UserNavigationViewItem { ViewModel: NavigationViewModelBase { Navigation: NavigationUserNode targetNavigation } } && _sourceNavigationViewModel is NavigationViewModelBase { Navigation: NavigationUserNode sourceNavigation })
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
    }
  }

  private void MainPage_SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    ViewModel.SearchNoteCommand?.Execute(args.QueryText);
  }
}

internal sealed partial class MainPage : Page
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, MessageToken>(this, MessageTokens.AppThmeChangedToken, new((recipient, message) => SetAppTheme(message.Value)));

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<WindowActivationState>, MessageToken>(this, MessageTokens.MainWindowActivationChangedToken, new((recipient, message) =>
    {
      if (message.Value == WindowActivationState.Deactivated)
      {
        TryGetWindowInfo(out _, out var appWindow);
        if (appWindow is not null)
        {
          var _inputNonClientPointerSource = InputNonClientPointerSource.GetForWindowId(appWindow.Id);
          _inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, null);
        }
        VisualStateManager.GoToState(this, "WindowDeactivated", false);
      }
      else
      {
        SetRegionsForCustomTitleBar();
        VisualStateManager.GoToState(this, "WindowActivated", false);
      }
    }));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}

internal sealed partial class MainPageNavigationViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? NavigationCoreNodeTemplate { get; set; }
  public DataTemplate? NavigationSeparatorTemplate { get; set; }
  public DataTemplate? NavigationUserCompositeNodeTemplate { get; set; }
  public DataTemplate? NavigationUserLeafNodeTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      CoreNavigationViewModel => NavigationCoreNodeTemplate,
      SeparatorNavigationViewModel => NavigationSeparatorTemplate,
      UserCompositeNavigationViewModel => NavigationUserCompositeNodeTemplate,
      UserLeafNavigationViewModel => NavigationUserLeafNodeTemplate,
      _ => null
    };
  }
}