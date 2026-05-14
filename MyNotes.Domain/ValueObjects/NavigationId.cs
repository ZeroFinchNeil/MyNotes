using System;
using System.Collections.Generic;

using MyNotes.Shared.Constants;

namespace MyNotes.Domain.ValueObjects;

internal readonly record struct NavigationId
{
  private static readonly Guid _empty = AppCoreNavigations.EmptyId;
  private static readonly Guid _root = AppCoreNavigations.RootId;
  private static readonly Guid _home = AppCoreNavigations.HomeId;
  private static readonly Guid _bookmarks = AppCoreNavigations.BookmarksId;
  private static readonly Guid _tags = AppCoreNavigations.TagsId;
  private static readonly Guid _trash = AppCoreNavigations.TrashId;
  private static readonly Guid _settings = AppCoreNavigations.SettingsId;
  private static readonly Guid _allowedLowerBound = AppCoreNavigations.AllowedLowerBound;

  public static bool IsValidId(Guid id) => id >= _allowedLowerBound;

  public static bool IsValidId(NavigationId navigationId) => IsValidId(navigationId.Value);

  public static NavigationId Empty { get; } = new(_empty);
  public static NavigationId UserRoot { get; } = new(_root);
  public static NavigationId Home { get; } = new(_home);
  public static NavigationId Bookmarks { get; } = new(_bookmarks);
  public static NavigationId Tags { get; } = new(_tags);
  public static NavigationId Trash { get; } = new(_trash);
  public static NavigationId Settings { get; } = new(_settings);

  private static readonly Dictionary<Guid, NavigationId> _reserved = new()
  {
    { _empty, Empty },
    { _root, UserRoot },
    { _home, Home },
    { _bookmarks, Bookmarks },
    { _tags, Tags },
    { _trash, Trash },
    { _settings, Settings },
  };

  public static NavigationId NewId()
  {
    Guid id;
    while (true)
    {
      id = Guid.NewGuid();
      if (IsValidId(id))
      {
        break;
      }
    }
    return new(id);
  }

  public Guid Value { get; init; }

  private NavigationId(Guid id) => Value = id;
  public NavigationId() => throw new InvalidOperationException("NavigationId has not been properly initialized.");

  public static NavigationId Create(Guid id) => IsValidId(id) ? new(id) : throw new ArgumentException("NavigationId cannot be generated because the given Guid is in a reserved range.", nameof(id));
  public static NavigationId Create(string id) => Create(Guid.Parse(id));

  public static NavigationId GetOrCreate(Guid id)
  {
    if (IsValidId(id))
    {
      return new(id);
    }
    else if (_reserved.TryGetValue(id, out var reserved))
    {
      return reserved;
    }
    else
    {
      return Empty;
    }
  }
}