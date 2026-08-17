using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract partial class UserNavigationViewModel : NavigationViewModelBase
{
  protected UserNavigationViewModel(NavigationUserNode navigation)
  {
    Navigation = navigation;
  }

  public override NavigationUserNode Navigation { get; }

  public abstract AsyncCommand AddListCommand { get; protected set; }
  public abstract AsyncCommand AddGroupCommand { get; protected set; }
  public abstract AsyncCommand ChangeTitleAndIconCommand { get; protected set; }
  public abstract AsyncCommand DeleteCommand { get; protected set; }
  public abstract AsyncCommand<NavigationUserCompositeNode> MoveToGroupCommand { get; protected set; }
  public abstract AsyncCommand SetAsStartPageCommand { get; protected set; }

  [ObservableProperty]
  public partial BitmapImage? IconImage { get; set; }

  public override string ToString() => $"{Navigation.Id.Value} ({Navigation.Title})";
}
