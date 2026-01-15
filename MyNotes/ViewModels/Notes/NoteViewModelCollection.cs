using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes;

internal sealed class NoteViewModelCollection : ObservableCollection<NoteViewModel>
{
  private List<NoteViewModel> _items => (List<NoteViewModel>)Items;
  public Comparer<NoteViewModel> Comparer { get; private set; }

  public NoteViewModelCollection(Comparer<Note> comparer) : base(new List<NoteViewModel>()) => Comparer = Comparer<NoteViewModel>.Create((x, y) => comparer.Compare(x.Note, y.Note));
  public NoteViewModelCollection(Comparer<NoteViewModel> comparer) : base(new List<NoteViewModel>()) => Comparer = comparer;

  public NoteViewModelCollection(IEnumerable<NoteViewModel> items, Comparer<Note> comparer) : base(new List<NoteViewModel>())
  {
    Comparer = Comparer<NoteViewModel>.Create((x, y) => comparer.Compare(x.Note, y.Note));
    _items.AddRange(items);
    _items.Sort(Comparer);
  }

  public NoteViewModelCollection(IEnumerable<NoteViewModel> items, Comparer<NoteViewModel> comparer) : base(new List<NoteViewModel>())
  {
    Comparer = comparer;
    _items.AddRange(items);
    _items.Sort(Comparer);
  }

  protected override void InsertItem(int index, NoteViewModel item)
  {
    int sortedIndex = GetSortedIndex(item);
    base.InsertItem(sortedIndex, item);
  }

  protected override void SetItem(int index, NoteViewModel item)
  {
    var oldItem = _items[index];
    if (Comparer.Compare(oldItem, item) == 0)
    {
      base.SetItem(index, item);
      return;
    }

    _items.RemoveAt(index);

    int newIndex = GetSortedIndex(item);
    _items.Insert(newIndex, item);

    if (index == newIndex)
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
    else
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, index));
  }

  protected override void MoveItem(int oldIndex, int newIndex) { }

  private int GetSortedIndex(NoteViewModel item)
  {
    int index = _items.BinarySearch(item, Comparer);
    return (index >= 0) ? index : ~index;
  }

  public void Rearrange(Comparer<Note> comparer)
  {
    Rearrange(Comparer<NoteViewModel>.Create((x, y) => comparer.Compare(x.Note, y.Note)));
  }

  public void ReorderItem(NoteViewModel item)
  {
    int oldIndex = _items.IndexOf(item);
    if (oldIndex < 0)
      return;

    int newIndex = GetSortedIndex(item);
    if (oldIndex == newIndex)
      return;

    if (oldIndex < newIndex)
      newIndex--;
    
    _items.RemoveAt(oldIndex);
    _items.Insert(newIndex, item);

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
    int count = _items.Count;
    NoteViewModel[] temp = new NoteViewModel[count];
    _items.CopyTo(0, temp, 0, count);

    Clear();

    Comparer = comparer;
    _items.AddRange(temp);
    _items.Sort(Comparer);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _items, 0));
  }
}
