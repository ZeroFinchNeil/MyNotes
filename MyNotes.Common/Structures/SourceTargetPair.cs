namespace MyNotes.Common.Structures;

internal record SourceTargetPair<TSource, TTarget>
{
  public required TSource Source { get; init; }
  public required TTarget Target { get; init; }
}
