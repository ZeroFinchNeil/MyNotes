using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Services.Settings;
using MyNotes.Common.Helpers;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Logging;
using MyNotes.Models.Navigations;
using MyNotes.Models.UI;
using MyNotes.Services.Windows;
using MyNotes.Shared.Constants;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations;
using MyNotes.Views.Windows;

using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class MainPage : Page
{
  private readonly MainViewModel ViewModel;
  private readonly SettingsService SettingsService;
  private readonly MainWindowService MainWindowService;
  private readonly AppLogger LoggingService;
  private readonly IServiceScope ServiceScope;

  private readonly NavigationId _initialNavigationId;

  #region Object Lifetime Management
  public MainPage(MainWindow mainWindow, NavigationId? initialNavigationId = null)
  {
    TrackReference();
    InitializeComponent();

    ServiceScope = App.Services.CreateScope();
    ViewModel = ServiceScope.ServiceProvider.GetRequiredService<MainViewModel>();
    SettingsService = ServiceScope.ServiceProvider.GetRequiredService<SettingsService>();
    MainWindowService = ServiceScope.ServiceProvider.GetRequiredService<MainWindowService>();
    LoggingService = ServiceScope.ServiceProvider.GetRequiredService<AppLogger>();

    mainWindow.SetTitleBar(MainPage_TitleBarGrid);

    // 시작 내비게이션(페이지) 설정
    _initialNavigationId = initialNavigationId ?? NavigationId.GetOrCreate(SettingsService.Load(AppSettingsDescriptors.InitialPageId));

    // 앱 테마 설정 
    var theme = (ElementTheme)SettingsService.Load(AppSettingsDescriptors.AppTheme);
    this.RequestedTheme = theme;

    // 메신저 등록
    RegisterMessengers();

    // 드래그 UI 타이머 등록
    SetDraggableNavigationTimer();

    this.Loaded += MainPage_Loaded;
    this.Unloaded += MainPage_Unloaded;

    MainPage_NavigationView.SelectionChanged += MainPage_NavigationView_SelectionChanged;
  }

  private void MainPage_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (args.SelectedItem is NavigationViewModelBase item)
    {
      string title = "NV";
      int index = ViewModel.MenuItems.IndexOf(item);

      if (index < 0)
      {
        Stack<UserGroupNavigationViewModel> stack = new();
        UserGroupNavigationViewModel group = ViewModel.UserRootNavigationViewModel;
        stack.Push(group);

        while (stack.Count > 0)
        {
          group = stack.Pop();
          foreach (var child in group.ChildNodeViewModels)
          {
            index = group.ChildNodeViewModels.IndexOf(item);
            if (index >= 0)
            {
              title = group.Navigation.Title;
              break;
            }
            if (child is UserGroupNavigationViewModel groupChild)
            {
              stack.Push(groupChild);
            }
          }

          if (index >= 0)
          {
            break;
          }
        }
      }

      Console.WriteLine("{0}: {1}", "group", title);
      Console.WriteLine("{0}: {1}", "index", index);
    }
  }

  private async void MainPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
    ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    ViewModel.NavigateTo(_initialNavigationId);
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
  #endregion

  #region 타이틀바 드래그 영역 조정
  private void MainPage_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void MainPage_TitleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void MainPage_BackButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void SetRegionsForCustomTitleBar()
  {
    if (MainWindowService.TryGetWindowInfo(this, out _, out var appWindow) && this.XamlRoot is XamlRoot xamlRoot)
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
    _preventNavigation = true;
    try
    {
      ViewModel.NavigateBack();
    }
    finally
    {
      _preventNavigation = false;
    }
  }

  private bool _preventNavigation = false;

  private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(MainViewModel.CurrentNavigationViewModel):
        if (ViewModel.CurrentNavigationViewModel is NavigationViewModelBase { Navigation: INavigation navigation })
        {
          switch (navigation)
          {
            case NavigationUserCompositeNode:
              return;
            case NavigationSearch search:
              MainPage_NavigationFrame.Navigate(search.PageType, navigation);
              return;
            case INavigationInitialTarget initialTarget:
              MainPage_NavigationFrame.Navigate(initialTarget.PageType, navigation);
              if (initialTarget is NavigationHome or NavigationBookmarks or NavigationUserLeafNode
                  && SettingsService.Load(AppSettingsDescriptors.InitialPageType) == (int)InitialPageType.LastOpened)
              {
                SettingsService.Save(AppSettingsDescriptors.InitialPageId, initialTarget.Id.Value);
              }
              break;
            case INavigationNode node:
              MainPage_NavigationFrame.Navigate(node.PageType, navigation);
              break;
            default:
              return;
          }

          if (!_preventNavigation)
          {
            ViewModel.NavigateTo(navigation);
          }
        }
        break;
    }
  }

  public void SetNavigation(NavigationId? navigationId) => ViewModel.NavigateTo(navigationId ?? _initialNavigationId);

  private void SetAppTheme(ElementTheme theme)
  {
    this.RequestedTheme = theme;

    if (MainWindowService.TryGetWindowInfo(this, out _, out var appWindow))
    {
      appWindow.TitleBar.PreferredTheme = theme switch
      {
        ElementTheme.Light => TitleBarTheme.Light,
        ElementTheme.Dark => TitleBarTheme.Dark,
        _ => TitleBarTheme.UseDefaultAppMode
      };
    }
  }

  #region Navigation Drag & Drop
  private NavigationViewModelBase? _dragSourceNavigationViewModel;
  private void MainPageUserNavigationViewItem_PresenterDragStarting(UIElement sender, DragStartingEventArgs args)
  {
    if (sender is UserNavigationViewItem { ViewModel: NavigationViewModelBase { Navigation: NavigationUserNode sourceNavigation } dragSourceViewModel })
    {
      _dragSourceNavigationViewModel = dragSourceViewModel;
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
    _expandableNavigation?.IsExpanded = !_expandableNavigation.IsExpanded;
    _expandableNavigation = null;
  }

  private readonly string _navigationFormatId = $"{AppStrings.PackageFamilyName}.NavigationUserNode.Id";
  private DragUISession? _dragUISession;
  private NavigationUserCompositeNode? _expandableNavigation;

  private async void MainPageUserNavigationViewItem_DragEnter(object sender, DragEventArgs e)
  {
    e.Handled = true;

    if (e.DataView.Contains(_navigationFormatId) && await e.DataView.GetDataAsync(_navigationFormatId) is string id)
    {
      if (sender is UserNavigationViewItem { ViewModel: NavigationViewModelBase { Navigation: NavigationUserNode hoveredTargetNavigation } }
          && _dragSourceNavigationViewModel is NavigationViewModelBase { Navigation: NavigationUserNode dragSourceNavigation })
      {
        bool canAcceptDrop = !(hoveredTargetNavigation == dragSourceNavigation || hoveredTargetNavigation.IsDescendantOf(dragSourceNavigation));

        _dragUISession = new()
        {
          FormatId = _navigationFormatId,
          DataView = id,
          DataPackageOperation = canAcceptDrop ? DataPackageOperation.Move : DataPackageOperation.None,
          DragUIOverrideCaption = canAcceptDrop ? "Move to this position" : "Prohibited from moving to itself or sub-items."
        };

        _dispatcherTimer.Stop();
        if (hoveredTargetNavigation is NavigationUserCompositeNode compositeNode)
        {
          _expandableNavigation = compositeNode;
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
      _dragUISession = null;
    }
  }

  private async void MainPageUserNavigationViewItem_Drop(object sender, DragEventArgs e)
  {
    e.Handled = true;
    _dispatcherTimer.Stop();
    _dragUISession?.Dispose();
    _dragUISession = null;

    if (sender is UserNavigationViewItem { ViewModel: NavigationViewModelBase { Navigation: NavigationUserNode dropTargetNavigation } }
        && _dragSourceNavigationViewModel is NavigationViewModelBase { Navigation: NavigationUserNode dragSourceNavigation })
    {
      await ViewModel.MoveNavigationAsync(new() { Source = dragSourceNavigation, Target = dropTargetNavigation });
    }
  }
  #endregion

  private async void MainPage_OpenDebugWindowMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    await App.Instance.OpenDebugWindow();
  }
}

#region Messengers
internal sealed partial class MainPage : Page
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, MessageToken>(this, AppMessageTokens.ChangeAppThemeToken, new((recipient, message) => SetAppTheme(message.Value)));

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<WindowActivationState>, MessageToken>(this, AppMessageTokens.MainWindowActivationChangedToken, new((recipient, message) =>
    {
      if (message.Value == WindowActivationState.Deactivated)
      {
        if (MainWindowService.TryGetWindowInfo(this, out _, out var appWindow))
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
#endregion

internal sealed partial class MainPageNavigationViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? CoreNavigationTemplate { get; set; }
  public DataTemplate? SeparatorNavigationTemplate { get; set; }
  public DataTemplate? UserRootGroupNavigationTemplate { get; set; }
  public DataTemplate? UserGroupNavigationTemplate { get; set; }
  public DataTemplate? UserListNavigationTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      CoreNavigationViewModel => CoreNavigationTemplate,
      SeparatorNavigationViewModel => SeparatorNavigationTemplate,
      UserRootGroupNavigationViewModel => UserRootGroupNavigationTemplate,
      UserGroupNavigationViewModel => UserGroupNavigationTemplate,
      UserListNavigationViewModel => UserListNavigationTemplate,
      _ => null
    };
  }
}