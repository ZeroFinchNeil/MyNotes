using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models;
using MyNotes.ViewModels;

using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Views.Windows;

public sealed partial class MainWindow : Window
{
  private readonly MainViewModel ViewModel;
  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;

  public MainWindow()
  {
    InitializeComponent();
    ViewModel = App.Instance.Services.GetRequiredService<MainViewModel>();

    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(MainWindow_TitleBarGrid);

    // DPI 스케일 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    double scaleFactor = Common.Interop.NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    _presenter = AppWindow.Presenter as OverlappedPresenter;
    _presenter?.PreferredMinimumWidth = (int)(600 * scaleFactor);
    _presenter?.PreferredMinimumHeight = (int)(600 * scaleFactor);

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
    AppWindow.ResizeClient(new((int)(600 * scaleFactor), (int)(800 * scaleFactor)));
  }

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    this.Activated -= MainWindow_Activated;
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

      RightPaddingColumn.Width = new GridLength(Math.Max(0, AppWindow.TitleBar.RightInset) / scaleFactor);
      LeftPaddingColumn.Width = new GridLength(Math.Max(0, AppWindow.TitleBar.LeftInset) / scaleFactor);

      var BackButtonPosition = MainWindow_BackButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_BackButton.ActualWidth, MainWindow_BackButton.ActualHeight));
      var PaneToggleButtonPosition = MainWindow_PaneToggleButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_PaneToggleButton.ActualWidth, MainWindow_PaneToggleButton.ActualHeight));
      var SearchBoxPosition = MainWindow_SearchAutoSuggestBox.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_SearchAutoSuggestBox.ActualWidth, MainWindow_SearchAutoSuggestBox.ActualHeight));

      RectInt32 BackButtonRect = GetRect(BackButtonPosition, scaleFactor);
      RectInt32 PaneToggleButtonRect = GetRect(PaneToggleButtonPosition, scaleFactor);
      RectInt32 SearchBoxRect = GetRect(SearchBoxPosition, scaleFactor);

      //Debug.WriteLine(BackButtonRect.X + ", " + PaneToggleButtonRect.X + ", " + SearchBoxRect.X);
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
        if (ViewModel.GetUserNode(n => n.Id == Guid.Parse(id)) is NavigationUserNode sourceNode &&
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

  private void MainWindow_MoveListsCancelButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(MainWindow_RootControl, "MoveListsPopupCollapsed", false);
  }

  private void MainWindow_MoveListsApplyButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(MainWindow_RootControl, "MoveListsPopupCollapsed", false);
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