using System;

using MyNotes.Common.Exceptions;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Domain.Entities.Navigations;

internal sealed class UserNavigation
{
  public NavigationId Id { get; init; }

  public NavigationId Parent { get; set; }

  public bool IsComposite { get; init; }

  public int Icon { get; set; }

  public string Title { get; set; }

  public bool IsDeleted { get; init; }

  public UserNavigation(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted)
  {
    Validate(id, parent, icon);

    Id = id;
    Parent = parent;
    IsComposite = isComposite;
    Icon = icon;
    Title = title;
    IsDeleted = isDeleted;
  }

  private static void Validate(NavigationId id, NavigationId parent, int icon)
  {
    if (!NavigationId.IsValidId(id))
    {
      throw new InvalidArgumentValueException("유효하지 않은 Id입니다.", nameof(id));
    }

    if (!NavigationId.IsValidId(parent))
    {
      throw new InvalidArgumentValueException("유효하지 않은 Parent Id입니다.", nameof(parent));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(icon, nameof(icon));
  }
}
