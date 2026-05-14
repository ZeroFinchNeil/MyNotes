using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteUpdateFields
{
  None,
  NavigationId,
  Created,
  Modified,
  Title,
  Body,
  BodyPlainText,
  IsBookmarked,
  IsDeleted
}
