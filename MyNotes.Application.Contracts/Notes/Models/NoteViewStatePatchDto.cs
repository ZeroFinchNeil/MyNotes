using System;
using System.Collections.Generic;
using System.Text;

using DotNext;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Common.Structures;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed record NoteViewStatePatchDto
{
  public required NoteId Id { get; init; }

  public Optional<bool> ShowBackgroundImage { get; init; }

  public Optional<int> BackgroundImageStretch { get; init; }

  public Optional<AlignmentPosition> BackgroundImageAlignment { get; init; }

  public Optional<double> BackgroundImageOpacity { get; init; }

  public Optional<double> BackgroundImageBlur { get; init; }

  public Optional<BackdropKind> BackdropKind { get; init; }

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
    ShowBackgroundImage.IsUndefined: true,
    BackgroundImageStretch.IsUndefined: true,
    BackgroundImageAlignment.IsUndefined: true,
    BackgroundImageOpacity.IsUndefined: true,
    BackgroundImageBlur.IsUndefined: true,
    BackdropKind.IsUndefined: true,
    BackdropTintOpacity.IsUndefined: true,
    BackdropLuminosityOpacity.IsUndefined: true,
    ShowImagePanel.IsUndefined: true,
    ImagePanelHeight.IsUndefined: true,
    Width.IsUndefined: true,
    Height.IsUndefined: true,
    PositionX.IsUndefined: true,
    PositionY.IsUndefined: true,
    IsTextEditorReadOnly.IsUndefined: true,
    IsWindowOpen.IsUndefined: true,
    IsAlwaysOnTop.IsUndefined: true
  };

  public NoteViewStatePatchDto Overlay(NoteViewStatePatchDto incoming) => this.Id == incoming.Id
    ? incoming.IsEmpty
      ? this
      : this with
      {
        ShowBackgroundImage = ShowBackgroundImage << incoming.ShowBackgroundImage,
        BackgroundImageStretch = BackgroundImageStretch << incoming.BackgroundImageStretch,
        BackgroundImageAlignment = BackgroundImageAlignment << incoming.BackgroundImageAlignment,
        BackgroundImageOpacity = BackgroundImageOpacity << incoming.BackgroundImageOpacity,
        BackgroundImageBlur = BackgroundImageBlur << incoming.BackgroundImageBlur,
        BackdropKind = BackdropKind << incoming.BackdropKind,
        BackdropTintOpacity = BackdropTintOpacity << incoming.BackdropTintOpacity,
        BackdropLuminosityOpacity = BackdropLuminosityOpacity << incoming.BackdropLuminosityOpacity,
        ShowImagePanel = ShowImagePanel << incoming.ShowImagePanel,
        ImagePanelHeight = ImagePanelHeight << incoming.ImagePanelHeight,
        Width = Width << incoming.Width,
        Height = Height << incoming.Height,
        PositionX = PositionX << incoming.PositionX,
        PositionY = PositionY << incoming.PositionY,
        IsTextEditorReadOnly = IsTextEditorReadOnly << incoming.IsTextEditorReadOnly,
        IsWindowOpen = IsWindowOpen << incoming.IsWindowOpen,
        IsAlwaysOnTop = IsAlwaysOnTop << incoming.IsAlwaysOnTop
      }
    : throw new ArgumentException("", nameof(incoming));

  public static NoteViewStatePatchDto Composite(params ReadOnlySpan<NoteViewStatePatchDto> patches)
  {
    if (patches.IsEmpty)
    {
      throw new ArgumentException("하나 이상의 Patch가 필요합니다.", nameof(patches));
    }

    var composite = patches[0];
    foreach (var patch in patches)
    {
      composite = composite.Overlay(patch);
    }

    return composite;
  }

  public static NoteViewStatePatchDto Composite(IEnumerable<NoteViewStatePatchDto> patches)
  {
    using var enumerator = patches.GetEnumerator();
    if (!enumerator.MoveNext())
    {
      throw new ArgumentException("하나 이상의 Patch가 필요합니다.", nameof(patches));
    }

    var composite = enumerator.Current;
    while (enumerator.MoveNext())
    {
      composite = composite.Overlay(enumerator.Current);
    }

    return composite;
  }

  public override string ToString()
  {
    StringBuilder sb = new();
    sb.AppendLine($"{nameof(NoteViewStateDto)} [ {nameof(Id)}: {Id} ]");
    if (ShowBackgroundImage.HasValue)
    {
      sb.AppendLine($"\t{nameof(ShowBackgroundImage)}: {ShowBackgroundImage}");
    }
    if (BackgroundImageStretch.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackgroundImageStretch)}: {BackgroundImageStretch}");
    }
    if (BackgroundImageAlignment.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackgroundImageAlignment)}: {BackgroundImageAlignment}");
    }
    if (BackgroundImageOpacity.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackgroundImageOpacity)}: {BackgroundImageOpacity}");
    }
    if (BackgroundImageBlur.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackgroundImageBlur)}: {BackgroundImageBlur}");
    }
    if (BackdropKind.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackdropKind)}: {BackdropKind}");
    }
    if (BackdropTintOpacity.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackdropTintOpacity)}: {BackdropTintOpacity}");
    }
    if (BackdropLuminosityOpacity.HasValue)
    {
      sb.AppendLine($"\t{nameof(BackdropLuminosityOpacity)}: {BackdropLuminosityOpacity}");
    }
    if (ShowImagePanel.HasValue)
    {
      sb.AppendLine($"\t{nameof(ShowImagePanel)}: {ShowImagePanel}");
    }
    if (ImagePanelHeight.HasValue)
    {
      sb.AppendLine($"\t{nameof(ImagePanelHeight)}: {ImagePanelHeight}");
    }
    if (Width.HasValue)
    {
      sb.AppendLine($"\t{nameof(Width)}: {Width}");
    }
    if (Height.HasValue)
    {
      sb.AppendLine($"\t{nameof(Height)}: {Height}");
    }
    if (PositionX.HasValue)
    {
      sb.AppendLine($"\t{nameof(PositionX)}: {PositionX}");
    }
    if (PositionY.HasValue)
    {
      sb.AppendLine($"\t{nameof(PositionY)}: {PositionY}");
    }
    if (IsTextEditorReadOnly.HasValue)
    {
      sb.AppendLine($"\t{nameof(IsTextEditorReadOnly)}: {IsTextEditorReadOnly}");
    }
    if (IsWindowOpen.HasValue)
    {
      sb.AppendLine($"\t{nameof(IsWindowOpen)}: {IsWindowOpen}");
    }
    if (IsAlwaysOnTop.HasValue)
    {
      sb.AppendLine($"\t{nameof(IsAlwaysOnTop)}: {IsAlwaysOnTop}");
    }
    return sb.ToString();
  }
}