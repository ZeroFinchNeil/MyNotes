using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteUpdateFields
{
  None,
  ParentId,
  Created,
  Modified,
  Title,
  Body,
  BodyPlainText,
  BackgroundColor,
  IsBookmarked,
  IsDeleted
}
