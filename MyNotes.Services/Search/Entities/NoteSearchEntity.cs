using System;

namespace MyNotes.Services.Search.Entities;

internal sealed class NoteSearchEntity : ISearchEntity<NoteSearchEntity>
{
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required string Body { get; set; }

  public bool Equals(NoteSearchEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteSearchEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
