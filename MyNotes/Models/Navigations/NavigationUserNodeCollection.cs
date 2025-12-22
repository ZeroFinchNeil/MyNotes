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
      this[oldIndex].Position = this[0].Position - 1;
    else if (newIndex == Count - 1)
      this[oldIndex].Position = this[^1].Position + 1;
    else
    {
      int offset = 1;
      int basePosition = this[newIndex].Position;
      int left, right;
      int hit;

      if (oldIndex < newIndex)
      {
        left = newIndex - 1;
        right = newIndex;
      }
      else if (oldIndex > newIndex)
      {
        left = newIndex;
        right = newIndex + 1;
      }
      else
        return;

      while (true)
      {
        if (basePosition - this[left].Position > offset)
        {
          hit = left;
          break;
        }
        else if (this[right].Position - basePosition > offset)
        {
          hit = right;
          break;
        }

        left--;
        right++;
        offset++;

        if (left == -1 || left == oldIndex - 1)
        {
          hit = left;
          break;
        }
        else if (right == oldIndex + 1 || right == Count)
        {
          hit = right;
          break;
        }
      }

      if (hit < newIndex)
      {
        for (int i = hit + 1; i < newIndex; i++)
          this[i].Position--;
        this[oldIndex].Position = basePosition - 1;
      }
      else if (hit > newIndex)
      {
        for (int i = newIndex; i < hit; i++)
          this[i].Position++;
        this[oldIndex].Position = basePosition;
      }
    }
    base.MoveItem(oldIndex, newIndex);
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
    }
    else if (index == 0)
      item.Position = this[0].Position - 1;
    else if (index == Count)
      item.Position = this[^1].Position + 1;
    else
    {
      int offset = 1;
      int basePosition = this[index].Position;
      int left = index - 1;
      int right = index;
      int hit;

      while (true)
      {
        if (basePosition - this[left].Position > offset)
        {
          hit = left;
          break;
        }
        else if (this[right].Position - basePosition > offset)
        {
          hit = right;
          break;
        }

        left--;
        right++;
        offset++;

        if (left == -1)
        {
          hit = left;
          break;
        }

        if (right == Count)
        {
          hit = right;
          break;
        }
      }

      if (hit < index)
      {
        for (int i = hit + 1; i < index; i++)
          this[i].Position--;
        item.Position = basePosition - 1;
      }
      else if (hit > index)
      {
        for (int i = index; i < hit; i++)
          this[i].Position++;
        item.Position = basePosition;
      }
    }

    base.InsertItem(index, item);
  }
}
