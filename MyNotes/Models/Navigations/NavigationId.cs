namespace MyNotes.Models.Navigations;

internal readonly record struct NavigationId
{
  private static readonly Guid _empty = Guid.Empty;
  private static readonly Guid _root = Guid.Parse("00000000-0000-0000-0000-000000000001");
  private static readonly Guid _home = Guid.Parse("00000000-0000-0000-0000-000000000002");
  private static readonly Guid _bookmarks = Guid.Parse("00000000-0000-0000-0000-000000000003");
  private static readonly Guid _tags = Guid.Parse("00000000-0000-0000-0000-000000000004");
  private static readonly Guid _trash = Guid.Parse("00000000-0000-0000-0000-000000000005");
  private static readonly Guid _settings = Guid.Parse("00000000-0000-0000-0000-000000000006");
  private static readonly Guid _allowedLowerBound = Guid.Parse("00000000-0000-0000-0000-000000000010");

  public static bool IsValidId(Guid id) => id >= _allowedLowerBound;

  public static NavigationId Empty { get; } = new(_empty);
  public static NavigationId UserRootNode { get; } = new(_root);
  public static NavigationId Home { get; } = new(_home);
  public static NavigationId Bookmarks { get; } = new(_bookmarks);
  public static NavigationId Tags { get; } = new(_tags);
  public static NavigationId Trash { get; } = new(_trash);
  public static NavigationId Settings { get; } = new(_settings);

  private static readonly Dictionary<Guid, NavigationId> _reserved = new()
  {
    { _empty, Empty },
    { _root, UserRootNode },
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
        break;
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
      return new(id);
    else if (_reserved.TryGetValue(id, out var reserved))
      return reserved;
    else
      return Empty;
  }
}