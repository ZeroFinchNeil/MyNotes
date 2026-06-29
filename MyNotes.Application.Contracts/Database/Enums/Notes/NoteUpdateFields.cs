using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteUpdateFields
{
  None = 0,
  ParentId = 1 << 0,
  Created = 1 << 1,
  Modified = 1 << 2,
  Title = 1 << 3,
  Body = 1 << 4,
  BodyPlainText = 1 << 5,
  BackgroundColor = 1 << 6,
  IsBookmarked = 1 << 7,
  IsDeleted = 1 << 8
}
