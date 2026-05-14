using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Infrastructure.Database.Entities.Notes;

internal sealed class NoteEntity : IDatabaseEntity<NoteEntity>
{
  [Key]
  public required Guid Id { get; init; }

  public required Guid Parent { get; set; }

  public required DateTimeOffset Created { get; init; }

  public required DateTimeOffset Modified { get; set; }

  public required string Title { get; set; }

  public required string Body { get; set; }

  public required string BackgroundColor { get; set; }

  public required bool IsBookmarked { get; set; }

  public required bool IsDeleted { get; set; }

  public bool Equals(NoteEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
