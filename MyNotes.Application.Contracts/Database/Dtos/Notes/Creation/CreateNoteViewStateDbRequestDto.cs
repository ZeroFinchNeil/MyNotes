using System.Collections.Generic;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;

internal class CreateNoteViewStateDbRequestDto
{
  public required NoteId Id { get; init; }

  public bool ShowBackgroundImage { get; init; }

  public string? BackgroundImagePath { get; init; }

  public double BackgroundImageOpacity { get; init; }

  public double BackgroundImageBlur { get; init; }

  public int BackdropKind { get; init; }

  public double BackdropTintOpacity { get; init; }

  public double BackdropLuminosityOpacity { get; init; }

  public IReadOnlyList<string> Images { get; init; } = [];

  public bool ShowImagePanel { get; init; }

  public double ImagePanelHeight { get; init; }

  public int Width { get; init; }

  public int Height { get; init; }

  public int PositionX { get; init; }

  public int PositionY { get; init; }

  public bool IsWindowOpen { get; init; }

  public bool IsAlwaysOnTop { get; init; }
}
