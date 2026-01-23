using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationSearch : ObservableObject, INavigation
{
  public required string Title
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required string SearchText
  {
    get;
    set => SetProperty(ref field, value);
  }

  public Type PageType { get; } = typeof(HomePage);

  public NavigationSearch() { }
}
