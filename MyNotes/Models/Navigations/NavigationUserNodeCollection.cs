namespace MyNotes.Models.Navigations;

internal sealed class NavigationUserNodeCollection : ObservableCollection<NavigationUserNode>
{
  private readonly NavigationUserCompositeNode _parent;

  public NavigationUserNodeCollection(NavigationUserCompositeNode parent) => _parent = parent;

  protected override void MoveItem(int oldIndex, int newIndex)
  {
    if (oldIndex < 0 || newIndex < 0
        || oldIndex >= Count || newIndex >= Count
        || oldIndex == newIndex)
    {
      return;
    }

    if (newIndex == 0)
    {
      this[oldIndex].Position = this[0].Position - 1;
      base.MoveItem(oldIndex, newIndex);
    }
    else if (newIndex == Count - 1)
    {
      this[oldIndex].Position = this[^1].Position + 1;
      base.MoveItem(oldIndex, newIndex);
    }
    else
    {
      base.MoveItem(oldIndex, newIndex);
      Reposition(newIndex);
    }
  }

  protected override void InsertItem(int index, NavigationUserNode item)
  {
    if (index < 0 || index > Count)
      return;

    if (item.Parent != _parent)
      item.Parent = _parent;

    if (Count == 0)
    {
      if (item.Position == int.MaxValue)
        item.Position = 0;
      base.InsertItem(index, item);
    }
    else if (index == 0)
    {
      item.Position = this[0].Position - 1;
      base.InsertItem(index, item);
    }
    else if (index == Count)
    {
      item.Position = this[^1].Position + 1;
      base.InsertItem(index, item);
    }
    else
    {
      base.InsertItem(index, item);
      Reposition(index);
    }
  }

  private void Reposition(int index)
  {
    if (index < 1 || index >= Count - 1)
      throw new IndexOutOfRangeException();

    int gap = 0;
    int hit;
    int leftIdx, rightIdx;
    int leftPos, rightPos;
    int midPos = this[index - 1].Position + (this[index + 1].Position - this[index - 1].Position) / 2;

    while (true)
    {
      gap++;
      leftIdx = index - gap;
      rightIdx = index + gap;

      if (leftIdx == -1)
      {
        hit = leftIdx;
        break;
      }
      else if (rightIdx == Count)
      {
        hit = rightIdx;
        break;
      }

      leftPos = this[leftIdx].Position;
      rightPos = this[rightIdx].Position;

      if (midPos - leftPos >= gap)
      {
        hit = leftIdx;
        break;
      }

      if (rightPos - midPos > gap)
      {
        hit = rightIdx;
        break;
      }
    }

    if (gap == 1)
    {
      this[index].Position = midPos;
    }
    else if (hit < index)
    {
      this[index].Position = this[index - 1].Position;
      for (int i = hit + 1; i < index; i++)
      {
        this[i].Position--;
      }
    }
    else if (hit > index)
    {
      this[index].Position = this[index + 1].Position;
      for (int i = index + 1; i < hit; i++)
      {
        this[i].Position++;
      }
    }
  }
}
