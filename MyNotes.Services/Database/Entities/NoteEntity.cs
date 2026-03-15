using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Services.Database.Entities;

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

  public required bool IsBackgroundImageVisible { get; set; }

  public required string? BackgroundImagePath { get; set; }

  public required double BackgroundImageOpacity { get; set; }

  public required double BackgroundImageBlur { get; set; }

  public required int BackdropKind { get; set; }

  public required double BackdropTintOpacity { get; set; }

  public required double BackdropLuminosityOpacity { get; set; }

  public required int Width { get; set; }

  public required int Height { get; set; }

  public required int PositionX { get; set; }

  public required int PositionY { get; set; }

  public required bool IsBookmarked { get; set; }

  public required bool IsDeleted { get; set; }

  public required bool IsWindowOpen { get; set; }

  public required bool IsAlwaysOnTop { get; set; }

  public bool Equals(NoteEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
