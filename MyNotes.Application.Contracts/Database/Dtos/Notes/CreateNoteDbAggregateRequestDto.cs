namespace MyNotes.Application.Contracts.Database.Dtos.Notes;

internal record CreateNoteDbAggregateRequestDto
{
  public required CreateNoteDbRequestDto CreateNoteDbRequestDto { get; init; }

  public required CreateNoteViewStateDbRequestDto CreateNoteViewStateDbRequestDto { get; init; }
}
