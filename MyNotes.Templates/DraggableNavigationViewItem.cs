using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

using Windows.Foundation;

namespace MyNotes.Templates;

public partial class DraggableNavigationViewItem : NavigationViewItem
{
  public DraggableNavigationViewItem()
  {
    DefaultStyleKey = typeof(NavigationViewItem);
    DragStartingWeakEventListner = new(this)
    {
      OnEventAction = (instance, source, args) => instance.PresenterDragStarting?.Invoke(instance, args),
      OnDetachAction = (weakEventListener) => Presenter?.DragStarting -= weakEventListener.OnEvent
    };
    DropCompletedWeakEventListner = new(this)
    {
      OnEventAction = (instance, source, args) => instance?.PresenterDropCompleted?.Invoke(instance, args),
      OnDetachAction = (weakEventListener) => Presenter?.DropCompleted -= weakEventListener.OnEvent
    };
    DragEnterWeakEventHandler = new(this)
    {
      OnEventAction = (instance, source, args) => VisualStateManager.GoToState(Presenter, "PointerOver", false),
      OnDetachAction = (weakEventListener) => DragEnter -= weakEventListener.OnEvent
    };
    DragLeaveWeakEventHandler = new(this)
    {
      OnEventAction = (instance, source, args) => VisualStateManager.GoToState(Presenter, "Normal", false),
      OnDetachAction = (weakEventListener) => DragLeave -= weakEventListener.OnEvent
    };

    this.Loaded += DraggableNavigationViewItem_Loaded;
    this.Unloaded += DraggableNavigationViewItem_Unloaded;
  }

  public event TypedEventHandler<UIElement, DragStartingEventArgs>? PresenterDragStarting;
  public event TypedEventHandler<UIElement, DropCompletedEventArgs>? PresenterDropCompleted;

  private NavigationViewItemPresenter? Presenter;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    Presenter = GetTemplateChild("NavigationViewItemPresenter") as NavigationViewItemPresenter;

    Presenter?.AllowDrop = this.AllowDrop;
    Presenter?.CanDrag = this.CanDrag;
  }

  private readonly WeakEventListener<DraggableNavigationViewItem, UIElement, DragStartingEventArgs> DragStartingWeakEventListner;
  private readonly WeakEventListener<DraggableNavigationViewItem, UIElement, DropCompletedEventArgs> DropCompletedWeakEventListner;
  private readonly WeakEventListener<DraggableNavigationViewItem, object, DragEventArgs> DragEnterWeakEventHandler;
  private readonly WeakEventListener<DraggableNavigationViewItem, object, DragEventArgs> DragLeaveWeakEventHandler;

  private void DraggableNavigationViewItem_Loaded(object sender, RoutedEventArgs e)
  {
    DetachWeakEventHandler();
    AttachWeakEventHandler();
  }

  private void DraggableNavigationViewItem_Unloaded(object sender, RoutedEventArgs e)
  {
    DetachWeakEventHandler();
  }

  private void AttachWeakEventHandler()
  {
    Presenter?.DragStarting += DragStartingWeakEventListner.OnEvent;
    Presenter?.DropCompleted += DropCompletedWeakEventListner.OnEvent;

    Presenter?.DragEnter += DragEnterWeakEventHandler.OnEvent;
    Presenter?.DragLeave += DragLeaveWeakEventHandler.OnEvent;
  }

  private void DetachWeakEventHandler()
  {
    Presenter?.DragStarting -= DragStartingWeakEventListner.OnEvent;
    Presenter?.DropCompleted -= DropCompletedWeakEventListner.OnEvent;

    Presenter?.DragEnter -= DragEnterWeakEventHandler.OnEvent;
    Presenter?.DragLeave -= DragLeaveWeakEventHandler.OnEvent;
  }
}
