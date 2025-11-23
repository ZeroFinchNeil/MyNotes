using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using CommunityToolkit.WinUI.Helpers;

using Windows.Foundation;
using System.Diagnostics;

namespace MyNotes.Templates;

public sealed partial class ReorderableNavigationViewItem : NavigationViewItem
{
  public ReorderableNavigationViewItem()
  {
    DefaultStyleKey = typeof(ReorderableNavigationViewItem);
  }

  public new event TypedEventHandler<UIElement, DragStartingEventArgs>? DragStarting;
  public new event TypedEventHandler<UIElement, DropCompletedEventArgs>? DropCompleted;

  private NavigationViewItemPresenter Presenter = null!;

  protected override void OnApplyTemplate()
  {
    Debug.WriteLine("OnApplyTemplate");
    base.OnApplyTemplate();

    Presenter = (NavigationViewItemPresenter)GetTemplateChild("NavigationViewItemPresenter");

    WeakEventListener<ReorderableNavigationViewItem, UIElement, DragStartingEventArgs> DragStartingEventListner = new(this)
    {
      OnEventAction = (instance, source, args) => instance.DragStarting?.Invoke(instance, args),
      OnDetachAction = (weakEventListener) => Presenter.DragStarting -= weakEventListener.OnEvent
    };
    Presenter.DragStarting += DragStartingEventListner.OnEvent;

    WeakEventListener<ReorderableNavigationViewItem, UIElement, DropCompletedEventArgs> DropCompletedEventListner = new(this)
    {
      OnEventAction = (instance, source, args) => instance?.DropCompleted?.Invoke(instance, args),
      OnDetachAction = (weakEventListener) => Presenter.DropCompleted -= weakEventListener.OnEvent
    };

    Presenter.DragStarting += DragStartingEventListner.OnEvent;
    Presenter.DropCompleted += DropCompletedEventListner.OnEvent;
  }
}
