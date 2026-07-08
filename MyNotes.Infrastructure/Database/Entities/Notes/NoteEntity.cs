using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Infrastructure.Database.Entities.Notes;

internal sealed class NoteEntity : IDatabaseEntity<NoteEntity>
{
  [Key, Required]
  public required Guid Id { get; init; }

  [Required]
  public required Guid Parent { get; set; }

  [Required]
  public required DateTimeOffset Created { get; init; }

  [Required]
  public required DateTimeOffset Modified { get; set; }

  [Required]
  public required string Title { get; set; }

  [Required]
  public required string Body { get; set; }

  [Required]
  public required string BackgroundColor { get; set; }

  [Required]
  public required bool IsBookmarked { get; set; }

  [Required]
  public required bool IsDeleted { get; set; }

  public bool Equals(NoteEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
