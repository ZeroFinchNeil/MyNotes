using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract partial class UserNavigationViewModel : NavigationViewModelBase
{
  protected UserNavigationViewModel(NavigationUserNode navigation)
  {
    Navigation = navigation;
  }

  public override NavigationUserNode Navigation { get; }

  public virtual AsyncCommand<NavigationUserNode>? AddListCommand { get; }
  public virtual Command<NavigationUserNode>? AddGroupCommand { get; }
  public virtual Command<NavigationUserNode>? UpdateCommand { get; }
  public virtual Command<NavigationUserNode>? DeleteCommand { get; }
  public virtual Command<SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode>>? MoveToGroupCommand { get; }
  public virtual Command<NavigationUserNode>? SetAsStartPageCommand { get; }

  [ObservableProperty]
  public partial BitmapImage? IconImage { get; set; }

  public override string ToString() => $"{Navigation.Id.Value} ({Navigation.Title})";
}
