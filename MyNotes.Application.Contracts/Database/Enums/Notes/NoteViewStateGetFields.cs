using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteViewStateGetFields
{
  None,
  ShowBackgroundImage,
  BackgroundImagePath,
  BackgroundImageOpacity,
  BackgroundImageBlur,
  BackdropKind,
  BackdropTintOpacity,
  BackdropLuminosityOpacity,
  Images,
  ShowImagePanel,
  ImagePanelHeight,
  Width,
  Height,
  PositionX,
  PositionY,
  IsWindowOpen,
  IsAlwaysOnTop
}
