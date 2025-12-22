using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

using Windows.Foundation;

namespace MyNotes.Templates;

public sealed partial class DraggableNavigationViewItem : NavigationViewItem
{
  public DraggableNavigationViewItem()
  {
    DefaultStyleKey = typeof(NavigationViewItem);
  }

  public new event TypedEventHandler<UIElement, DragStartingEventArgs>? DragStarting;
  public new event TypedEventHandler<UIElement, DropCompletedEventArgs>? DropCompleted;

  private NavigationViewItemPresenter? Presenter;
  
  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    Presenter = GetTemplateChild("NavigationViewItemPresenter") as NavigationViewItemPresenter;

    Presenter?.AllowDrop = this.AllowDrop;
    Presenter?.CanDrag = this.CanDrag;

    WeakEventListener<DraggableNavigationViewItem, UIElement, DragStartingEventArgs> DragStartingEventListner = new(this)
    {
      OnEventAction = (instance, source, args) => instance.DragStarting?.Invoke(instance, args),
      OnDetachAction = (weakEventListener) => Presenter?.DragStarting -= weakEventListener.OnEvent
    };
    Presenter?.DragStarting += DragStartingEventListner.OnEvent;

    WeakEventListener<DraggableNavigationViewItem, UIElement, DropCompletedEventArgs> DropCompletedEventListner = new(this)
    {
      OnEventAction = (instance, source, args) => instance?.DropCompleted?.Invoke(instance, args),
      OnDetachAction = (weakEventListener) => Presenter?.DropCompleted -= weakEventListener.OnEvent
    };

    Presenter?.DragStarting += DragStartingEventListner.OnEvent;
    Presenter?.DropCompleted += DropCompletedEventListner.OnEvent;
    
    Presenter?.DragEnter += (s, e) => VisualStateManager.GoToState(Presenter, "PointerOver", false);
    Presenter?.DragLeave += (s, e) => VisualStateManager.GoToState(Presenter, "Normal", false);
  }
}
