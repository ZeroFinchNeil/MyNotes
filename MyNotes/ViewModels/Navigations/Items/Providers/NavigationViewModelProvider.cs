using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;
using MyNotes.Models.Navigations.Core;
using MyNotes.Models.Navigations.User;

namespace MyNotes.ViewModels.Navigations.Items.Providers;

internal sealed class NavigationViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigation, NavigationViewModelBase>
{
  private readonly ConcurrentDictionary<INavigation, ViewModelCache> ResolveTable = new();
  private readonly Func<INavigation, ViewModelCache> _cacheFactory = navigation => new
  (
    referenceCounterFactory: () => new ReferenceCounter<NavigationViewModelBase>(navigation switch
    {
      NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(serviceProvider, navigation),
      NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(serviceProvider, navigation),
      NavigationUserRootNode => ActivatorUtilities.CreateInstance<UserRootGroupNavigationViewModel>(serviceProvider, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserGroupNavigationViewModel>(serviceProvider, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserListNavigationViewModel>(serviceProvider, navigation),
      _ => throw new ArgumentException("Invalid navigation")
    })
  );

  public IViewModelLease<NavigationViewModelBase> Resolve(INavigation navigation)
  {
    while (true)
    {
      var cache = ResolveTable.GetOrAdd(navigation, _cacheFactory);

      lock (cache.SyncRoot)
      {
        if (ResolveTable.TryGetValue(navigation, out ViewModelCache? currentCache) && ReferenceEquals(currentCache, cache))
        {
          if (cache.ReferenceCounter.TryAcquire(out var viewModel))
          {
            return CreateLease(navigation, viewModel, cache);
          }

          ResolveTable.TryRemove(navigation, out _);
        }
      }
    }
  }

  private ViewModelLease CreateLease(INavigation navigation, NavigationViewModelBase viewmodel, ViewModelCache cache) => new ViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseAction = () =>
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.ReleaseOrDetach(out _))
        {
          viewmodel.Dispose();
          ResolveTable.TryRemove(navigation, out _);
        }
      }
    }
  };

  public IViewModelLease<NavigationViewModelBase>? Acquire(INavigation navigation)
  {
    if (ResolveTable.TryGetValue(navigation, out var cache))
    {
      lock (cache.SyncRoot)
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
        {
          if (!viewmodel.Disposed)
          {
            return CreateLease(navigation, viewmodel, cache);
          }
          else
          {
            ResolveTable.TryRemove(navigation, out _);
          }
        }
      }
    }
    return null;
  }

  public IViewModelLease<NavigationViewModelBase>? Acquire(NavigationId navigationId)
  {
    INavigationNode? navigation = null;
    foreach (var key in ResolveTable.Keys.OfType<INavigationNode>().ToArray())
    {
      if (key.Id == navigationId)
      {
        navigation = key;
        break;
      }
    }
    return navigation is not null ? Acquire(navigation) : null;
  }

  private sealed class ViewModelLease() : IViewModelLease<NavigationViewModelBase>
  {
    public required NavigationViewModelBase ViewModel { get; init; }
    public required Action ReleaseAction { get; init; }

    public bool Disposed { get; private set; }

    private void Dispose(bool disposing)
    {
      if (Disposed)
      {
        return;
      }

      if (disposing)
      {
        ReleaseAction.Invoke();
      }

      Disposed = true;
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
  private sealed class ViewModelCache(Func<ReferenceCounter<NavigationViewModelBase>> referenceCounterFactory)
  {
    public Lock SyncRoot { get; } = new();

    private readonly Lazy<ReferenceCounter<NavigationViewModelBase>> _referenceCounter = new(referenceCounterFactory);

    public ReferenceCounter<NavigationViewModelBase> ReferenceCounter => _referenceCounter.Value;
  }
}