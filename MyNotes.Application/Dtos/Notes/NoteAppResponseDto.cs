using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Notes;

internal sealed record NoteAppResponseDto
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId { get; init; }

  public required DateTimeOffset Created { get; init; }

  public required DateTimeOffset Modified { get; init; }

  public required string Title { get; init; }

  public required string Body { get; init; }

  public required string BodyPlainText { get; init; }

  public required bool IsBookmarked { get; init; }

  public required bool IsDeleted { get; init; }

  public required bool ShowBackgroundImage { get; init; }

  public required string? BackgroundImagePath { get; init; }

  public required double BackgroundImageOpacity { get; init; }

  public required double BackgroundImageBlur { get; init; }

  public required int BackdropKind { get; init; }

  public required double BackdropTintOpacity { get; init; }

  public required double BackdropLuminosityOpacity { get; init; }

  public required IReadOnlyList<string> Images { get; init; }

  public required bool ShowImagePanel { get; init; }

  public required double ImagePanelHeight { get; init; }

  public required int Width { get; init; }

  public required int Height { get; init; }

  public required int PositionX { get; init; }

  public required int PositionY { get; init; }

  public required bool IsWindowOpen { get; init; }

  public required bool IsAlwaysOnTop { get; init; }
}

/*
NoteAppResponseDto dto = new()
{
  Id = ,
  NavigationId = ,
  Created = ,
  Modified = ,
  Title = ,
  Body = ,
  BodyPlainText = ,
  IsBookmarked = ,
  IsDeleted = ,
  ShowBackgroundImage = ,
  BackgroundImagePath = ,
  BackgroundImageOpacity = ,
  BackgroundImageBlur = ,
  BackdropKind = ,
  BackdropTintOpacity = ,
  BackdropLuminosityOpacity = ,
  Images = ,
  ShowImagePanel = ,
  ImagePanelHeight = ,
  Width = ,
  Height = ,
  PositionX = ,
  PositionY = ,
  IsWindowOpen = ,
  IsAlwaysOnTop = ,
};
*/