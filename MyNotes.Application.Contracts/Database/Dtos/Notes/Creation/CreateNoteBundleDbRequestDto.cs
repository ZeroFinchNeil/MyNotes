using System;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;

internal sealed record CreateNoteBundleDbRequestDto
{
  public NoteId Id => NoteDto.Id;

  public CreateNoteDbRequestDto NoteDto { get; }

  public CreateNoteViewStateDbRequestDto ViewStateDto { get; }

  public CreateNoteBundleDbRequestDto(CreateNoteDbRequestDto noteDto, CreateNoteViewStateDbRequestDto viewStateDto)
  {
    if (noteDto.Id != viewStateDto.Id)
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(noteDto));
    }

    NoteDto = noteDto;
    ViewStateDto = viewStateDto;
  }
}
