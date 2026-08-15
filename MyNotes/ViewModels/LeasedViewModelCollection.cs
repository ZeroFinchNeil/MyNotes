namespace MyNotes.ViewModels;

internal partial class LeasedViewModelCollection<TViewModel, TCollection> : IDisposable where TViewModel : ViewModelBase, IDisposable where TCollection : Collection<TViewModel>
{
  private readonly Dictionary<TViewModel, IViewModelLease<TViewModel>> _leases;
  protected TCollection InnerViewModels { get; }
  public IReadOnlyList<TViewModel> ViewModels { get; }

  public LeasedViewModelCollection(Func<IEnumerable<TViewModel>, TCollection> collectionFactory)
  {
    _leases = new(ReferenceEqualityComparer.Instance);
    InnerViewModels = collectionFactory([]);

    ViewModels = InnerViewModels switch
    {
      ObservableCollection<TViewModel> inner => new ReadOnlyObservableCollection<TViewModel>(inner),
      _ => InnerViewModels.AsReadOnly()
    };
  }

  public LeasedViewModelCollection(IEnumerable<IViewModelLease<TViewModel>> leases, Func<IEnumerable<TViewModel>, TCollection> collectionFactory)
  {
    _leases = new(leases.ToDictionary(lease => lease.ViewModel), ReferenceEqualityComparer.Instance);
    InnerViewModels = collectionFactory(_leases.Keys);

    ViewModels = InnerViewModels switch
    {
      ObservableCollection<TViewModel> inner => new ReadOnlyObservableCollection<TViewModel>(inner),
      _ => InnerViewModels.AsReadOnly()
    };
  }

  public virtual void Add(IViewModelLease<TViewModel> lease)
  {
    var viewmodel = lease.ViewModel;
    if (_leases.TryAdd(viewmodel, lease))
    {
      InnerViewModels.Add(viewmodel);
      return;
    }

    lease.Dispose();
    throw new InvalidOperationException("동일한 ViewModel 인스턴스가 이미 존재합니다.");
  }

  public virtual void Insert(int index, IViewModelLease<TViewModel> lease)
  {
    var viewmodel = lease.ViewModel;
    if (_leases.TryAdd(viewmodel, lease))
    {
      InnerViewModels.Insert(index, viewmodel);
      return;
    }

    lease.Dispose();
    throw new InvalidOperationException("동일한 ViewModel 인스턴스가 이미 존재합니다.");
  }

  public virtual bool Remove(TViewModel viewmodel)
  {
    if (_leases.Remove(viewmodel, out var lease))
    {
      lease.Dispose();
    }

    return InnerViewModels.Remove(viewmodel);
  }

  public virtual void RemoveAt(int index)
  {
    var viewmodel = InnerViewModels[index];
    if (_leases.Remove(viewmodel, out var lease))
    {
      lease.Dispose();
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

  public virtual bool Replace(TViewModel oldViewModel, IViewModelLease<TViewModel> newLease)
  {
    var newViewModel = newLease.ViewModel;
    if (oldViewModel == newViewModel)
    {
      newLease.Dispose();
      throw new InvalidOperationException("동일한 ViewModel 인스턴스로 교체할 수 없습니다.");
    }

    if (_leases.TryGetValue(newViewModel, out _))
    {
      newLease.Dispose();
      throw new InvalidOperationException("동일한 ViewModel 인스턴스가 이미 존재합니다.");
    }

    if (_leases.Remove(oldViewModel, out var oldLease))
    {
      int index = InnerViewModels.IndexOf(oldViewModel);
      if (index >= 0 && _leases.TryAdd(newViewModel, newLease))
      {
        InnerViewModels[index] = newViewModel;
        oldLease.Dispose();
        return true;
      }
    }
    return false;
  }

  public virtual void Clear()
  {
    foreach (var viewmodel in InnerViewModels)
    {
      if (_leases.Remove(viewmodel, out var lease))
      {
        lease.Dispose();
      }
    }
    InnerViewModels.Clear();
  }

  public bool Contains(TViewModel viewmodel) => _leases.ContainsKey(viewmodel) && ViewModels.Contains(viewmodel);

  public void Dispose()
  {
    foreach (var lease in _leases.Values)
    {
      lease.Dispose();
    }
    InnerViewModels.Clear();
  }
}