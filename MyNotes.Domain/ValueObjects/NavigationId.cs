using System;
using System.Collections.Generic;

using MyNotes.Shared.Constants;

namespace MyNotes.Domain.ValueObjects;

internal readonly record struct NavigationId
{
  public static NavigationId Empty { get; } = new(AppNavigationGuids.EmptyId);
  public static NavigationId UserRoot { get; } = new(AppNavigationGuids.RootId);
  public static NavigationId Home { get; } = new(AppNavigationGuids.HomeId);
  public static NavigationId Bookmarks { get; } = new(AppNavigationGuids.BookmarksId);
  public static NavigationId Tags { get; } = new(AppNavigationGuids.TagsId);
  public static NavigationId Trash { get; } = new(AppNavigationGuids.TrashId);
  public static NavigationId Settings { get; } = new(AppNavigationGuids.SettingsId);

  private static readonly Dictionary<Guid, NavigationId> _reserved = new()
  {
    { Empty.Value, Empty },
    { UserRoot.Value, UserRoot },
    { Home.Value, Home },
    { Bookmarks.Value, Bookmarks },
    { Tags.Value, Tags },
    { Trash.Value, Trash },
    { Settings.Value, Settings },
  };

  private static readonly Guid _allowedLowerBound = AppNavigationGuids.AllowedLowerBound;
  private static readonly Guid _allowedUpperBound = AppNavigationGuids.AllowedUpperBound;

  public static bool IsValidId(Guid id) => id >= _allowedLowerBound && id <= _allowedUpperBound;

  public static bool IsValidParentId(Guid id) => id == UserRoot.Value || IsValidId(id);

  public static bool IsValidId(NavigationId navigationId) => IsValidId(navigationId.Value);

  public static bool IsValidParentId(NavigationId navigationId) => IsValidParentId(navigationId.Value);

  public static NavigationId NewId()
  {
    Guid id;
    do
    {
      id = Guid.NewGuid();
    }
    while (!IsValidId(id));
    return new(id);
  }

  public Guid Value { get; init; }

  private NavigationId(Guid id) => Value = id;

  public NavigationId() => throw new InvalidOperationException("NavigationId has not been properly initialized.");

  public static NavigationId Create(Guid id) => IsValidParentId(id) ? new(id) : throw new ArgumentException("NavigationId cannot be generated because the given Guid is in a reserved range.", nameof(id));

  public static NavigationId Create(string id) => Create(Guid.Parse(id));

  public static NavigationId? CreateOrDefault(Guid? id) => id is Guid value
    ? IsValidId(value) 
      ? new(value) : null
    : null;

  public static NavigationId GetOrCreate(Guid id)
  {
    if (IsValidId(id))
    {
      return new(id);
    }

    if (id > _allowedUpperBound)
    {
      throw new ArgumentException("NavigationId cannot be created because the given Guid is in the upper reserved range.", nameof(id));
    }

    if (_reserved.TryGetValue(id, out var reserved))
    {
      return reserved;
    }

    return Empty;
  }
}