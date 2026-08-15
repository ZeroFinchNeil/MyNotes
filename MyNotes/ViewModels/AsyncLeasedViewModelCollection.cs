using System.Collections.Concurrent;

namespace MyNotes.ViewModels;

internal partial class AsyncLeasedViewModelCollection<TViewModel, TCollection> : IAsyncDisposable where TViewModel : ViewModelBase, IAsyncDisposable where TCollection : Collection<TViewModel>
{
  private readonly ConcurrentDictionary<TViewModel, IAsyncViewModelLease<TViewModel>> _leases;
  protected TCollection InnerViewModels { get; }
  public IReadOnlyList<TViewModel> ViewModels { get; }

  public AsyncLeasedViewModelCollection(Func<IEnumerable<TViewModel>, TCollection> collectionFactory)
  {
    _leases = new(ReferenceEqualityComparer.Instance);
    InnerViewModels = collectionFactory([]);

    ViewModels = InnerViewModels switch
    {
      ObservableCollection<TViewModel> inner => new ReadOnlyObservableCollection<TViewModel>(inner),
      _ => InnerViewModels.AsReadOnly()
    };
  }

  public AsyncLeasedViewModelCollection(IEnumerable<IAsyncViewModelLease<TViewModel>> leases, Func<IEnumerable<TViewModel>, TCollection> collectionFactory)
  {
    _leases = new(leases.ToDictionary(lease => lease.ViewModel), ReferenceEqualityComparer.Instance);
    InnerViewModels = collectionFactory(_leases.Keys);

    ViewModels = InnerViewModels switch
    {
      ObservableCollection<TViewModel> inner => new ReadOnlyObservableCollection<TViewModel>(inner),
      _ => InnerViewModels.AsReadOnly()
    };
  }

  public virtual async Task AddAsync(IAsyncViewModelLease<TViewModel> lease)
  {
    var viewmodel = lease.ViewModel;
    if (_leases.TryAdd(viewmodel, lease))
    {
      InnerViewModels.Add(viewmodel);
      return;
    }

    await lease.DisposeAsync();
    throw new InvalidOperationException("동일한 ViewModel 인스턴스가 이미 존재합니다.");
  }

  public virtual async Task Insert(int index, IAsyncViewModelLease<TViewModel> lease)
  {
    var viewmodel = lease.ViewModel;
    if (_leases.TryAdd(viewmodel, lease))
    {
      InnerViewModels.Insert(index, viewmodel);
      return;
    }

    await lease.DisposeAsync();
    throw new InvalidOperationException("동일한 ViewModel 인스턴스가 이미 존재합니다.");
  }

  public virtual async Task<bool> RemoveAsync(TViewModel viewmodel)
  {
    if (_leases.TryRemove(viewmodel, out var lease))
    {
      await lease.DisposeAsync();
    }

    return InnerViewModels.Remove(viewmodel);
  }

  public virtual async Task RemoveAtAsync(int index)
  {
    var viewmodel = InnerViewModels[index];
    if (_leases.Remove(viewmodel, out var lease))
    {
      await lease.DisposeAsync();
    }
    InnerViewModels.RemoveAt(index);
  }

  public virtual void Move(int oldIndex, int newIndex)
  {
    if (oldIndex < 0 || newIndex < 0 || oldIndex >= InnerViewModels.Count || newIndex >= InnerViewModels.Count)
    {
      throw new InvalidOperationException();
    }

    if (oldIndex == newIndex)
    {
      return;
    }
    var viewmodel = InnerViewModels[oldIndex];
    InnerViewModels.RemoveAt(oldIndex);
    InnerViewModels.Insert(newIndex, viewmodel);
  }

  public virtual async Task<bool> ReplaceAsync(TViewModel oldViewModel, IAsyncViewModelLease<TViewModel> newLease)
  {
    var newViewModel = newLease.ViewModel;
    if (oldViewModel == newViewModel)
    {
      await newLease.DisposeAsync();
      throw new InvalidOperationException("동일한 ViewModel 인스턴스로 교체할 수 없습니다.");
    }

    if (_leases.TryGetValue(newViewModel, out _))
    {
      await newLease.DisposeAsync();
      throw new InvalidOperationException("동일한 ViewModel 인스턴스가 이미 존재합니다.");
    }

    if (_leases.TryRemove(oldViewModel, out var oldLease))
    {
      int index = InnerViewModels.IndexOf(oldViewModel);
      if (index >= 0 && _leases.TryAdd(newViewModel, newLease))
      {
        InnerViewModels[index] = newViewModel;
        await oldLease.DisposeAsync();
        return true;
      }
    }
    return false;
  }

  public virtual async Task ClearAsync()
  {
    foreach (var viewmodel in InnerViewModels)
    {
      if (_leases.TryRemove(viewmodel, out var lease))
      {
        await lease.DisposeAsync();
      }
    }
    InnerViewModels.Clear();
  }

  public bool ContainsAsync(TViewModel viewmodel) => _leases.ContainsKey(viewmodel) && ViewModels.Contains(viewmodel);

  public async ValueTask DisposeAsync()
  {
    foreach (var lease in _leases.Values)
    {
      await lease.DisposeAsync();
    }
    InnerViewModels.Clear();
  }
}