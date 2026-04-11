using MyNotes.Models.Navigations;

namespace MyNotes.Services.Navigations;

/// <summary>
/// 내비게이션 및 탐색 관리를 제공합니다.
/// </summary>
/// <remarks>
/// 이 서비스는 기본 및 사용자 내비게이션 컬렉션 객체를 포함하고 내비게이션 스택을 관리하며 내비게이션 변경에 대한 이벤트를 노출합니다. 또한 특정 노드로 이동, 히스토리를 통해 뒤로 이동, 내비게이션 상태 초기화를 지원합니다.
/// </remarks>
internal sealed partial class NavigationService : IDisposable
{
  public ImmutableList<INavigation> PrimaryCoreNavigations { get; } = [NavigationHome.Instance, NavigationBookmarks.Instance, new NavigationSeparator()];
  public NavigationUserRootNode UserRootNavigation { get; } = NavigationUserRootNode.Instance;
  public ImmutableList<INavigation> SecondaryCoreNavigations { get; } = [new NavigationSeparator(), NavigationTrash.Instance, NavigationSettings.Instance];
  public IReadOnlyList<NavigationUserNode> UserCompositeNavigations => UserRootNavigation.FindDescendants(node => node is NavigationUserCompositeNode, true);
  public IReadOnlyList<NavigationUserNode> UserLeafNavigations => UserRootNavigation.FindDescendants(node => node is NavigationUserLeafNode, false);

  #region Object Lifetime Management
  public NavigationService()
  {
  }

  public bool Disposed { get; private set; }

  public void Dispose()
  {
    if (Disposed)
    {
      return;
    }

    Disposed = true;
  }
  #endregion

  public INavigation? CurrentNavigation
  {
    get;
    private set
    {
      if (field != value)
      {
        field = value;
        CurrentNavigationChanged?.Invoke(this, value);
      }
    }
  }

  public event TypedEventHandler<object, INavigation?>? CurrentNavigationChanged;

  public Stack<INavigation> NavigationBackStack { get; } = new();

  public void NavigateTo(INavigation navigation)
  {
    if (CurrentNavigation != navigation)
    {
      if (CurrentNavigation is not null)
      {
        NavigationBackStack.Push(CurrentNavigation);
      }

      CurrentNavigation = navigation;
    }
  }

  public void NavigateBack()
  {
    if (NavigationBackStack.Count > 0)
    {
      CurrentNavigation = NavigationBackStack.Pop();
    }
  }

  public void ResetNavigation()
  {
    NavigationBackStack.Clear();
    CurrentNavigation = null;
  }
}