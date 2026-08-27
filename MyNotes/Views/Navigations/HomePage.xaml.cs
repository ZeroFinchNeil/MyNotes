using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations.Core;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Navigations.Items;
using MyNotes.ViewModels.Navigations.Items.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class HomePage : Page
{
  private NavigationHome? Navigation;

  private IViewModelLease<NavigationViewModelBase>? ViewModelLease;

  public HomePage()
  {
    TrackReference();
    InitializeComponent();
    this.Loaded += HomePage_Loaded;
    this.Unloaded += HomePage_Unloaded;
  }

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationHome navigation)
    {
      Navigation = navigation;
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();
      //var noteListViewModelProvider = App.Services.GetRequiredService<NotePreviewListViewModelProvider>();

      ViewModelLease = navigationViewModelProvider.Acquire(Navigation);
      //NotePreviewListViewModelLease = await noteListViewModelProvider.ResolveAsync(Navigation);
    }
  }

  protected override async void OnNavigatedFrom(NavigationEventArgs e)
  {
    ViewModelLease?.Dispose();
    //if (NotePreviewListViewModelLease is not null)
    //{
    //  await NotePreviewListViewModelLease.DisposeAsync();
    //}
  }

  private async void HomePage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void HomePage_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}
