namespace MyNotes.Models.Navigation;

internal class NavigationId : IEquatable<NavigationId>
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
  private NavigationId(Guid id) => Value = id;

  public static NavigationId Create(Guid id) => IsValidId(id) ? new(id) : throw new ArgumentException("");
  public static NavigationId Create(string id) => Create(Guid.Parse(id));

  public Guid Value { get; init; }

  public static bool operator ==(NavigationId id1, NavigationId id2) => id1.Equals(id2);
  public static bool operator !=(NavigationId id1, NavigationId id2) => !id1.Equals(id2);

  public bool Equals(NavigationId? other) => other is not null && other.Value == Value;
  public override bool Equals(object? obj) => obj is NavigationId navigationId && navigationId.Value == Value;

  public override int GetHashCode() => Value.GetHashCode();

  public override string ToString() => Value.ToString();
}