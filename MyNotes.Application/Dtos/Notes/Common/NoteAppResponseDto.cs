using MyNotes.Domain.ValueObjects;

using Windows.UI;

namespace MyNotes.Application.Dtos.Notes.Common;

internal sealed record NoteAppResponseDto
{
  public required NoteId Id { get; init; }

  public required NavigationId NavigationId { get; init; }

  public required DateTimeOffset Created { get; init; }

  public required DateTimeOffset Modified { get; init; }

  public required string Title { get; init; }

  public required string Body { get; init; }

  public required string BodyPlainText { get; init; }

  public required string BackgroundColor { get; init; }

  public required bool IsBookmarked { get; init; }

  public required bool IsDeleted { get; init; }
}