namespace MyNotes.Models.Media;

internal sealed record ImageCollectionKey
{
  public required Guid Value { get; init; }
}