using DotNext;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Notes.Modification;

internal sealed record UpdateNoteViewStateAppRequestDto
{
  public required NoteId Id { get; init; }

  public Optional<bool> ShowBackgroundImage { get; init; }

  public Optional<int> BackgroundImageStretch { get; init; }

  public Optional<int> BackgroundImageAlignment { get; init; }

  public Optional<double> BackgroundImageOpacity { get; init; }

  public Optional<double> BackgroundImageBlur { get; init; }

  public Optional<int> BackdropKind { get; init; }

  public Optional<double> BackdropTintOpacity { get; init; }

  public Optional<double> BackdropLuminosityOpacity { get; init; }

  public Optional<bool> ShowImagePanel { get; init; }

  public Optional<double> ImagePanelHeight { get; init; }

  public Optional<int> Width { get; init; }

  public Optional<int> Height { get; init; }

  public Optional<int> PositionX { get; init; }

  public Optional<int> PositionY { get; init; }

  public Optional<bool> IsTextEditorReadOnly { get; init; }

  public Optional<bool> IsWindowOpen { get; init; }

  public Optional<bool> IsAlwaysOnTop { get; init; }

  public bool IsEmpty => this is
  {
    ShowBackgroundImage.IsUndefined : true,
    BackgroundImageStretch.IsUndefined: true,
    BackgroundImageAlignment.IsUndefined: true,
    BackgroundImageOpacity.IsUndefined : true,
    BackgroundImageBlur.IsUndefined : true,
    BackdropKind.IsUndefined : true,
    BackdropTintOpacity.IsUndefined : true,
    BackdropLuminosityOpacity.IsUndefined : true,
    ShowImagePanel.IsUndefined : true,
    ImagePanelHeight.IsUndefined : true,
    Width.IsUndefined : true,
    Height.IsUndefined : true,
    PositionX.IsUndefined : true,
    PositionY.IsUndefined : true,
    IsTextEditorReadOnly.IsUndefined: true,
    IsWindowOpen.IsUndefined : true,
    IsAlwaysOnTop.IsUndefined : true
  };
}