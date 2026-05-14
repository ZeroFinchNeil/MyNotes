namespace MyNotes.Application.Contracts.Database.Dtos.Notes;

internal record NoteDbAggregateResponseDto
{
  public required NoteDbResponseDto NoteDbResponseDto { get; init; }

  public required NoteViewStateDbResponseDto NoteViewStateDbResponseDto { get; init; }
}