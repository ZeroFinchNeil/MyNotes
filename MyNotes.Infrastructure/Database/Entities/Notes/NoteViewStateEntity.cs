using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNotes.Infrastructure.Database.Entities.Notes;

internal sealed class NoteViewStateEntity : IDatabaseEntity<NoteViewStateEntity>
{
  [Key]
  public required Guid Id { get; init; }

  [ForeignKey(nameof(Id))]
  public NoteEntity? Note { get; init; }

  public required bool ShowBackgroundImage { get; set; }

  public required string? BackgroundImagePath { get; set; }

  public required double BackgroundImageOpacity { get; set; }

  public required double BackgroundImageBlur { get; set; }

  public required int BackdropKind { get; set; }

  public required double BackdropTintOpacity { get; set; }

  public required double BackdropLuminosityOpacity { get; set; }

  public required string Images { get; set; }

  public required bool ShowImagePanel { get; set; }

  public required double ImagePanelHeight { get; set; }

  public required int Width { get; set; }

  public required int Height { get; set; }

  public required int PositionX { get; set; }

  public required int PositionY { get; set; }

  public required bool IsWindowOpen { get; set; }

  public required bool IsAlwaysOnTop { get; set; }

  public bool Equals(NoteViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
