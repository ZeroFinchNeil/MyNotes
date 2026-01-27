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

  public virtual Command<NavigationViewModelBase>? AddListCommand { get; }
  public virtual Command<NavigationViewModelBase>? AddGroupCommand { get; }
  public virtual Command<NavigationViewModelBase>? UpdateCommand { get; }
  public virtual Command<NavigationViewModelBase>? DeleteCommand { get; }
  public virtual Command<SourceTargetPair<NavigationViewModelBase, NavigationViewModelBase>>? MoveToGroupCommand { get; }
  public virtual Command<NavigationViewModelBase>? SetAsStartPageCommand { get; }

  public BitmapImage? IconImage
  {
    get;
    set => SetProperty(ref field, value);
  }
}
