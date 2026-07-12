using MyNotes.Domain.ValueObjects;

using Windows.Graphics;

namespace MyNotes.Application.Dtos.Notes.Creation;

internal sealed record CreateNoteAppRequestDto
{
  public required NavigationId NavigationId { get; init; }

  public required SizeInt32 Size { get; init; }

  public required PointInt32 Position { get; init; }
}