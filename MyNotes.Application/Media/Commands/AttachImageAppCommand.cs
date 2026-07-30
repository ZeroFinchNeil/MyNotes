using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Media.Commands;

internal sealed record AttachImageAppCommand
{
  public required ImageId Id { get; init; }

  public required NoteId NoteId { get; init; }

  public required string FileNameWithExtension { get; init; }
}