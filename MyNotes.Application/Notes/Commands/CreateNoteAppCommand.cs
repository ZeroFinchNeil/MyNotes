using MyNotes.Domain.Navigations;

using Windows.Graphics;

namespace MyNotes.Application.Notes.Commands;

internal sealed record CreateNoteAppCommand
{
  public required NavigationId NavigationId { get; init; }

  public required SizeInt32 Size { get; init; }

  public required PointInt32 Position { get; init; }
}