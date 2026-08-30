using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Infrastructure.Database.Entities.Notes;

internal sealed class NoteImageEntity : IDatabaseEntity<NoteImageEntity>
{
  [Key, Required]
  public required Guid Id { get; init; }

  [Required]
  public required Guid NoteId { get; init; }

  [Required]
  public required string OriginalFileName { get; init; }

  [Required]
  public required string StoredExtension { get; init; }

  [Required]
  public required int Position { get; set; }

  public bool Equals(NoteImageEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteImageEntity);
  public override int GetHashCode() => Id.GetHashCode();
}