using System;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Common;

internal record NoteBundleDbResponseDto
{
  public NoteId Id => NoteDto.Id;

  public NoteDbResponseDto NoteDto { get; }

  public NoteViewStateDbResponseDto ViewStateDto { get; }

  public NoteBundleDbResponseDto(NoteDbResponseDto noteDto, NoteViewStateDbResponseDto viewStateDto)
  {
    if (noteDto.Id != viewStateDto.Id)
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(noteDto));
    }

    NoteDto = noteDto;
    ViewStateDto = viewStateDto;
  }
}