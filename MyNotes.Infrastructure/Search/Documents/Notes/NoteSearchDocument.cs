using System;

namespace MyNotes.Infrastructure.Search.Documents.Notes;

internal sealed class NoteSearchDocument : ISearchDocument<NoteSearchDocument>
{
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required string Body { get; set; }

  public bool Equals(NoteSearchDocument? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteSearchDocument);
  public override int GetHashCode() => Id.GetHashCode();
}
