namespace MyNotes.Models.Navigations;

internal readonly record struct NavigationId
{
  public static NavigationId Empty { get; } = new(Guid.Empty);
  public static NavigationId UserRootNode { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));
  public static NavigationId Home { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000008"));
  public static NavigationId Bookmarks { get; } = new(Guid.Parse("00000000-0000-0000-0000-000000000009"));
  public static NavigationId Tags { get; } = new(Guid.Parse("00000000-0000-0000-0000-00000000000a"));

  private static readonly Guid _lowerBound = Guid.Parse("00000000-0000-0000-0000-000000000010");
  private static bool IsValidId(Guid id) => id >= _lowerBound;

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

  public static NavigationId Create(Guid id) => IsValidId(id) ? new(id) : throw new ArgumentException("");
  public static NavigationId Create(string id) => Create(Guid.Parse(id));
}