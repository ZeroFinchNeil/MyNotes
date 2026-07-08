using System;

using MyNotes.Common.Exceptions;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Domain.Entities.Navigations;

internal sealed class UserNavigation
{
  public NavigationId Id { get; init; }

  private NavigationId _parent;
  public NavigationId Parent
  {
    get => _parent;
    set => SetProperty(ref _parent, value);
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

  public UserNavigation(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted)
  {
    Id = id;
    _parent = parent;
    IsComposite = isComposite;
    _icon = icon;
    _title = title;
    _isDeleted = isDeleted;

    ValidateProperties();
  }

  private static void Validate(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted)
  {
    if (!NavigationId.IsValidId(id))
    {
      throw new InvalidArgumentValueException("유효하지 않은 Id입니다.", nameof(id));
    }

    if (!NavigationId.IsValidParentId(parent))
    {
      throw new InvalidArgumentValueException("유효하지 않은 Parent Id입니다.", nameof(parent));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(icon, nameof(icon));
  }

  private bool SetProperty<T>(ref T f, T v)
  {
    if (f is null || !f.Equals(v))
    {
      f = v;
      ValidateProperties();
      return true;
    }

    return false;
  }

  private void ValidateProperties() => Validate(Id, Parent, IsComposite, Icon, Title, IsDeleted);
}
