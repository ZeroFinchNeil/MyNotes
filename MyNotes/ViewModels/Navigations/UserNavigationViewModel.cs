using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract partial class UserNavigationViewModel : NavigationViewModelBase
{
  public UserNavigationViewModel(NavigationUserNode navigation)
  {
    Navigation = navigation;
  }

  public override NavigationUserNode Navigation { get; }

  public virtual Command<NavigationUserNode>? AddListCommand { get; }
  public virtual Command<NavigationUserNode>? AddGroupCommand { get; }
  public virtual Command<NavigationUserNode>? UpdateCommand { get; }
  public virtual Command<NavigationUserNode>? DeleteCommand { get; }
  public virtual Command<SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode>>? MoveToGroupCommand { get; }
  public virtual Command<NavigationUserNode>? SetAsStartPageCommand { get; }

  public BitmapImage? IconImage
  {
    get;
    set => SetProperty(ref field, value);
  }
}
