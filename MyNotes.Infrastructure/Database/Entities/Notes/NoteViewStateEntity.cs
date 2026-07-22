using System;
using System.ComponentModel.DataAnnotations;

namespace MyNotes.Infrastructure.Database.Entities.Notes;

internal sealed class NoteViewStateEntity : IDatabaseEntity<NoteViewStateEntity>
{
  [Key, Required]
  public required Guid Id { get; init; }

  [Required]
  public required bool ShowBackgroundImage { get; set; }

  [Required]
  public required int BackgroundImageStretch { get; set; }

  [Required]
  public required int BackgroundImageAlignment { get; set; }

  [Required]
  public required double BackgroundImageOpacity { get; set; }

  [Required]
  public required double BackgroundImageBlur { get; set; }

  [Required]
  public required int BackdropKind { get; set; }

  [Required]
  public required double BackdropTintOpacity { get; set; }

  [Required]
  public required double BackdropLuminosityOpacity { get; set; }

  [Required]
  public required bool ShowImagePanel { get; set; }

  [Required]
  public required double ImagePanelHeight { get; set; }

  [Required]
  public required int Width { get; set; }

  [Required]
  public required int Height { get; set; }

  [Required]
  public required int PositionX { get; set; }

  [Required]
  public required int PositionY { get; set; }

  [Required]
  public required bool IsTextEditorReadOnly { get; set; }

  [Required]
  public required bool IsWindowOpen { get; set; }

  [Required]
  public required bool IsAlwaysOnTop { get; set; }

  public bool Equals(NoteViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
