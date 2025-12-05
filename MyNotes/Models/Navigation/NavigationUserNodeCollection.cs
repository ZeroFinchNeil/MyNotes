namespace MyNotes.Models.Navigation;

internal sealed class NavigationUserNodeCollection : ObservableCollection<NavigationUserNode>
{
  protected override void InsertItem(int index, NavigationUserNode item)
  {
    if (index < 0 || index > Count)
      return;

    if (Count == 0)
      item.Position = 0;
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
          hit = -1;
          break;
        }
        
        if (right == Count)
        {
          hit = Count;
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
