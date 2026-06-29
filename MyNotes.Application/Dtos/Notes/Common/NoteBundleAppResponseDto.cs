using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Notes.Common;

internal sealed record NoteBundleAppResponseDto
{
  public NoteId Id => NoteDto.Id;

  public NoteAppResponseDto NoteDto { get; }

  public NoteViewStateAppResponseDto ViewStateDto { get; }

  public NoteBundleAppResponseDto(NoteAppResponseDto noteDto, NoteViewStateAppResponseDto viewStateDto)
  {
    if (noteDto.Id != viewStateDto.Id)
    {
      throw new ArgumentException("두 Dto의 Id가 일치하지 않습니다.", nameof(noteDto));
    }

    NoteDto = noteDto;
    ViewStateDto = viewStateDto;
  }
}