using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes;

[Debugging.ReferenceTracker]
internal sealed partial class NoteViewModelCollection : ObservableCollection<NoteViewModel>
{
  private List<NoteViewModel> Inner => (List<NoteViewModel>)Items;
  public Comparer<NoteViewModel> Comparer { get; private set; }

  #region Object Lifetime Management
  private NoteViewModelCollection() : base(new List<NoteViewModel>())
  {
    TrackReference();
    Comparer = null!;
  }

  public NoteViewModelCollection(Comparer<NoteModel> comparer) : this() => Comparer = Comparer<NoteViewModel>.Create((x, y) => comparer.Compare(x.Note, y.Note));
  public NoteViewModelCollection(Comparer<NoteViewModel> comparer) : this() => Comparer = comparer;

  public NoteViewModelCollection(IEnumerable<NoteViewModel> items, Comparer<NoteModel> comparer) : this()
  {
    Comparer = Comparer<NoteViewModel>.Create((x, y) => comparer.Compare(x.Note, y.Note));
    Inner.AddRange(items);
    Inner.Sort(Comparer);
  }

  public NoteViewModelCollection(IEnumerable<NoteViewModel> items, Comparer<NoteViewModel> comparer) : this()
  {
    Comparer = comparer;
    Inner.AddRange(items);
    Inner.Sort(Comparer);
  }
  #endregion

  protected override void InsertItem(int index, NoteViewModel item)
  {
    int sortedIndex = GetSortedIndex(item);
    base.InsertItem(sortedIndex, item);
  }

  protected override void SetItem(int index, NoteViewModel item)
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

  private int GetSortedIndex(NoteViewModel item)
  {
    int index = Inner.BinarySearch(item, Comparer);
    return (index >= 0) ? index : ~index;
  }

  public void Rearrange(Comparer<NoteModel> comparer)
  {
    Rearrange(Comparer<NoteViewModel>.Create((x, y) => comparer.Compare(x.Note, y.Note)));
  }

  public void ReorderItem(NoteViewModel item)
  {
    int oldIndex = Inner.IndexOf(item);
    if (oldIndex < 0)
    {
      return;
    }

    int newIndex = GetSortedIndex(item);
    if (oldIndex == newIndex)
    {
      return;
    }

    if (oldIndex < newIndex)
    {
      newIndex--;
    }

    Inner.RemoveAt(oldIndex);
    Inner.Insert(newIndex, item);

    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
  }

  /// Reconfigures (replaces) the current sorting comparer for the collection,
  /// and rearranges all items to match the new sorting rule.
  /// After calling this method, every add/insert operation will use the new comparer
  /// to maintain correct sorted order automatically.
  /// 지정한 Comparer로 컬렉션의 정렬 규칙 자체를 재조정하고
  /// 모든 항목을 새 Comparer 기준으로 다시 배열합니다.
  /// 이후 추가되는 항목도 이 Comparer에 따라 자동으로 정렬됩니다.
  public void Rearrange(Comparer<NoteViewModel> comparer)
  {
    int count = Inner.Count;
    NoteViewModel[] temp = new NoteViewModel[count];
    Inner.CopyTo(0, temp, 0, count);

    Clear();

    Comparer = comparer;
    Inner.AddRange(temp);
    Inner.Sort(Comparer);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Inner, 0));
  }
}
