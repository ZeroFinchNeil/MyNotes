using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MyNotes.Common.Collections;

public sealed class SortedObservableCollection<T> : ObservableCollection<T>
{
  private List<T> Inner => (List<T>)Items;
  public Comparer<T> Comparer { get; private set; }

  #region Object Lifetime Management
  private SortedObservableCollection() : base(new List<T>())
  {
    Comparer = null!;
  }

  public SortedObservableCollection(Comparer<T> comparer) : this() => Comparer = comparer;

  public SortedObservableCollection(IEnumerable<T> items, Comparer<T> comparer) : this()
  {
    Comparer = comparer;
    Inner.AddRange(items);
    Inner.Sort(Comparer);
  }
  #endregion

  protected override void InsertItem(int index, T item)
  {
    int sortedIndex = GetSortedIndex(item);
    base.InsertItem(sortedIndex, item);
  }

  protected override void SetItem(int index, T item)
  {
    var oldItem = Inner[index];
    if (Comparer.Compare(oldItem, item) == 0)
    {
      base.SetItem(index, item);
      return;
    }

    Inner.RemoveAt(index);

    int newIndex = GetSortedIndex(item);
    Inner.Insert(newIndex, item);

    if (index == newIndex)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
    }
    else
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, index));
    }
  }

  protected override void MoveItem(int oldIndex, int newIndex) { }

  private int GetSortedIndex(T item)
  {
    int index = Inner.BinarySearch(item, Comparer);
    return (index >= 0) ? index : ~index;
  }

  public void ReorderItem(T item)
  {
    int oldIndex = Inner.IndexOf(item);
    if (oldIndex < 0)
    {
      return;
    }
    Inner.RemoveAt(oldIndex);

    int newIndex = GetSortedIndex(item);
    Inner.Insert(newIndex, item);

    if (oldIndex != newIndex)
    {
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }
  }

  /// Reconfigures (replaces) the current sorting comparer for the collection,
  /// and rearranges all items to match the new sorting rule.
  /// After calling this method, every add/insert operation will use the new comparer
  /// to maintain correct sorted order automatically.
  /// 지정한 Comparer로 컬렉션의 정렬 규칙 자체를 재조정하고
  /// 모든 항목을 새 Comparer 기준으로 다시 배열합니다.
  /// 이후 추가되는 항목도 이 Comparer에 따라 자동으로 정렬됩니다.
  public void Rearrange(Comparer<T> comparer)
  {
    int count = Inner.Count;
    T[] temp = new T[count];
    Inner.CopyTo(0, temp, 0, count);

    Clear();

    Comparer = comparer;
    Inner.AddRange(temp);
    Inner.Sort(Comparer);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Inner, 0));
  }
}