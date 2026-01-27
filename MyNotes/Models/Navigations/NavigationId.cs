namespace MyNotes.Models.Navigations;

internal readonly record struct NavigationId
{
  public static NavigationId Empty { get; } = new(Guid.Empty);
  public static NavigationId UserRootNode { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
  public static NavigationId Home { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000008"));
  public static NavigationId Bookmarks { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000009"));
  public static NavigationId Tags { get; } = new(Guid.Parse("00000000-0000-0000-0000-00000000000a"));

  private static readonly Dictionary<Guid, NavigationId> _reserved = new()
  {
    { Guid.Empty, Empty },
    { Guid.Parse("00000000-0000-0000-0000-000000000001"), UserRootNode },
    { Guid.Parse("00000000-0000-0000-0000-000000000008"), Home },
    { Guid.Parse("00000000-0000-0000-0000-000000000009"), Bookmarks },
    { Guid.Parse("00000000-0000-0000-0000-00000000000a"), Tags },
  };

  private static readonly Guid _allowedLowerBound = Guid.Parse("00000000-0000-0000-0000-000000000010");
  private static bool IsValidId(Guid id) => id >= _allowedLowerBound;

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
      throw new ArgumentException("NavigationId cannot be generated.", nameof(id));
  }
}