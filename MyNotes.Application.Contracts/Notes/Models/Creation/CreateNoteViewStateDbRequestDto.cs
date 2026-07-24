using System.Collections.Generic;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Notes.Models.Creation;

internal class CreateNoteViewStateDbRequestDto
{
  public required NoteId Id { get; init; }

  public required bool ShowBackgroundImage { get; init; }

  public required int BackgroundImageStretch { get; init; }

  public required int BackgroundImageAlignment { get; init; }

  public required double BackgroundImageOpacity { get; init; }

  public required double BackgroundImageBlur { get; init; }

  public required int BackdropKind { get; init; }

  public required double BackdropTintOpacity { get; init; }

  public required double BackdropLuminosityOpacity { get; init; }

  public required bool ShowImagePanel { get; init; }

  public required double ImagePanelHeight { get; init; }

  public required int Width { get; init; }

  public required int Height { get; init; }

  public required int PositionX { get; init; }

  public required int PositionY { get; init; }

  public required bool IsTextEditorReadOnly { get; init; }

  public required bool IsWindowOpen { get; init; }

  public required bool IsAlwaysOnTop { get; init; }
}
