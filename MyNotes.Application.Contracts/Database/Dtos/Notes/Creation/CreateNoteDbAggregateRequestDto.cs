namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;

internal record CreateNoteDbAggregateRequestDto
{
  public required CreateNoteDbRequestDto CreateNoteDbRequestDto { get; init; }

  public required CreateNoteViewStateDbRequestDto CreateNoteViewStateDbRequestDto { get; init; }
}
