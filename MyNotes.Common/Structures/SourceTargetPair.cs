namespace MyNotes.Common.Structures;

public record SourceTargetPair<TSource, TTarget>
{
  public required TSource Source { get; init; }
  public required TTarget Target { get; init; }
}
