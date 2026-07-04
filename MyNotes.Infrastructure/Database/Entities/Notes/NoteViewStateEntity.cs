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

  public required bool ShowBackgroundImage { get; init; }

  public required string? BackgroundImagePath { get; init; }

  public required double BackgroundImageOpacity { get; init; }

  public required double BackgroundImageBlur { get; init; }

  public required int BackdropKind { get; init; }

  public required double BackdropTintOpacity { get; init; }

  public required double BackdropLuminosityOpacity { get; init; }

  public required string Images { get; init; }

  public required bool ShowImagePanel { get; init; }

  public required double ImagePanelHeight { get; init; }

  public required int Width { get; init; }

  public required int Height { get; init; }

  public required int PositionX { get; init; }

  public required int PositionY { get; init; }

  public required bool IsWindowOpen { get; init; }

  public required bool IsAlwaysOnTop { get; init; }

  public bool Equals(NoteViewStateEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NoteViewStateEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
