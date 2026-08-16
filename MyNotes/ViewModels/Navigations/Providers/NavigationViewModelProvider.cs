using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;

namespace MyNotes.ViewModels.Navigations.Providers;

internal sealed class NavigationViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<INavigation, NavigationViewModelBase>
{
  private readonly ConcurrentDictionary<INavigation, NavigationViewModelCache> ResolveTable = new();
  private readonly Func<INavigation, NavigationViewModelCache> _cacheFactory = navigation => new
  (
    referenceCounterFactory: () => new ReferenceCounter<NavigationViewModelBase>(navigation switch
    {
      NavigationCoreNode => ActivatorUtilities.CreateInstance<CoreNavigationViewModel>(serviceProvider, navigation),
      NavigationSeparator => ActivatorUtilities.CreateInstance<SeparatorNavigationViewModel>(serviceProvider, navigation),
      NavigationUserRootNode => ActivatorUtilities.CreateInstance<UserRootGroupNavigationViewModel>(serviceProvider, navigation),
      NavigationUserCompositeNode => ActivatorUtilities.CreateInstance<UserGroupNavigationViewModel>(serviceProvider, navigation),
      NavigationUserLeafNode => ActivatorUtilities.CreateInstance<UserListNavigationViewModel>(serviceProvider, navigation),
      NavigationSearch => ActivatorUtilities.CreateInstance<SearchNavigationViewModel>(serviceProvider, navigation),
      _ => throw new ArgumentException("Invalid navigation")
    })
  );

  public IViewModelLease<NavigationViewModelBase> Resolve(INavigation navigation)
  {
    var cache = ResolveTable.GetOrAdd(navigation, _cacheFactory.Invoke);

    lock (cache.SyncRoot)
    {
      if (cache.ReferenceCounter.TryAcquire(out var viewmodel))
      {
        return CreateLease(navigation, viewmodel, cache);
      }
    }

    NavigationViewModelCache newCache = _cacheFactory(navigation);

    lock (newCache.SyncRoot)
    {
      ResolveTable.AddOrUpdate(navigation, newCache, (k, v) => v = newCache);
      return newCache.ReferenceCounter.TryAcquire(out var viewmodel) ? CreateLease(navigation, viewmodel, newCache) : throw new InvalidOperationException();
    }
  }

  private NavigationViewModelLease CreateLease(INavigation navigation, NavigationViewModelBase viewmodel, NavigationViewModelCache cache) => new NavigationViewModelLease()
  {
    ViewModel = viewmodel,
    ReleaseFunc = () => Release(navigation, cache)
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

  private bool Release(INavigation navigation, NavigationViewModelCache cache)
  {
    lock (cache.SyncRoot)
    {
      if (cache.ReferenceCounter.ReleaseOrDetach(out var lease))
      {
        lease.Dispose();
        ResolveTable.TryRemove(navigation, out _);
        return true;
      }
      return false;
    }
  }

  private sealed class NavigationViewModelLease() : IViewModelLease<NavigationViewModelBase>
  {
    public required NavigationViewModelBase ViewModel { get; init; }
    public Func<bool>? ReleaseFunc { get; init; }

    public bool Disposed { get; private set; }

    private void Dispose(bool disposing)
    {
      if (Disposed)
      {
        return;
      }

      if (disposing)
      {
        if (ReleaseFunc is null || ReleaseFunc.Invoke())
        {
          ViewModel.Dispose();
        }
      }

      Disposed = true;
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
  private sealed class NavigationViewModelCache(Func<ReferenceCounter<NavigationViewModelBase>> referenceCounterFactory)
  {
    public Lock SyncRoot { get; } = new();

    private readonly Lazy<ReferenceCounter<NavigationViewModelBase>> _referenceCounter = new(referenceCounterFactory);

    public ReferenceCounter<NavigationViewModelBase> ReferenceCounter => _referenceCounter.Value;
  }
}