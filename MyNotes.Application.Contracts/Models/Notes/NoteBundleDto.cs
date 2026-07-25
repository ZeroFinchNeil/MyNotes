using System;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Notes;

internal sealed record NoteBundleDto
{
  public NoteId Id => NoteDto.Id;

  public NoteDto NoteDto { get; }

  public NoteViewStateDto NoteViewStateDto { get; }

  public NoteBundleDto(NoteDto noteDto, NoteViewStateDto noteViewStateDto)
  {
    if (noteDto.Id != noteViewStateDto.Id)
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(noteDto));
    }

    NoteDto = noteDto;
    NoteViewStateDto = noteViewStateDto;
  }
}