using System;

namespace MyNotes.Domain.Navigations;

internal sealed class Navigation
{
  public NavigationId Id { get; init; }

  private NavigationId _parentId;
  public NavigationId ParentId
  {
    get => _parentId;
    set => SetProperty(ref _parentId, value);
  }

  public bool IsComposite { get; init; }

  private int _icon;
  public int Icon
  {
    get => _icon;
    set => SetProperty(ref _icon, value);
  }

  private string _title;
  public string Title
  {
    get => _title;
    set => SetProperty(ref _title, value);
  }

  private bool _isDeleted;
  public bool IsDeleted
  {
    get => _isDeleted;
    set => SetProperty(ref _isDeleted, value);
  }

  public Navigation(NavigationId id, NavigationId parentId, bool isComposite, int icon, string title, bool isDeleted)
  {
    Id = id;
    _parentId = parentId;
    IsComposite = isComposite;
    _icon = icon;
    _title = title;
    _isDeleted = isDeleted;

    ValidateProperties();
  }

  private static void Validate(NavigationId id, NavigationId parentId, bool isComposite, int icon, string title, bool isDeleted)
  {
    if (!NavigationId.IsValidId(id))
    {
      throw new ArgumentException("유효하지 않은 Id입니다.", nameof(id));
    }

    if (!NavigationId.IsValidParentId(parentId))
    {
      throw new ArgumentException("유효하지 않은 Parent Id입니다.", nameof(parentId));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(icon, nameof(icon));
  }

  private bool SetProperty<T>(ref T propertyField, T newValue)
  {
    if (propertyField is null || !propertyField.Equals(newValue))
    {
      T oldValue = propertyField;
      propertyField = newValue;
      try
      {
        ValidateProperties();
      }
      catch
      {
        propertyField = oldValue;
        throw;
      }
      return true;
    }

    return false;
  }

  private void ValidateProperties() => Validate(Id, ParentId, IsComposite, Icon, Title, IsDeleted);
}
