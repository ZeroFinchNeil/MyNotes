using System;

namespace MyNotes.Application.Contracts.Enums.Notes;

[Flags]
internal enum NoteViewStateGetFields
{
  None = 0,
  ShowBackgroundImage = 1 << 0,
  BackgroundImagePath = 1 << 1,
  BackgroundImageOpacity = 1 << 2,
  BackgroundImageBlur = 1 << 3,
  BackdropKind = 1 << 4,
  BackdropTintOpacity = 1 << 5,
  BackdropLuminosityOpacity = 1 << 6,
  Images = 1 << 7,
  ShowImagePanel = 1 << 8,
  ImagePanelHeight = 1 << 9,
  Width = 1 << 10,
  Height = 1 << 11,
  PositionX = 1 << 12,
  PositionY = 1 << 13,
  IsWindowOpen = 1 << 14,
  IsAlwaysOnTop = 1 << 15
}
