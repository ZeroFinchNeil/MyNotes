using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations;

internal abstract partial class UserNavigationViewModel : NavigationViewModelBase
{
  public override NavigationUserNode? Navigation => base.Navigation as NavigationUserNode;

  public virtual Command<NavigationViewModelBase>? AddListCommand { get; }
  public virtual Command<NavigationViewModelBase>? AddGroupCommand { get; }
  public virtual Command<NavigationViewModelBase>? UpdateCommand { get; }
  public virtual Command<NavigationViewModelBase>? DeleteCommand { get; }
  public virtual Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)>? MoveToGroupCommand { get; }
}
