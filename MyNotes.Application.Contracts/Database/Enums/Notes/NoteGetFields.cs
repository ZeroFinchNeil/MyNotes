using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteGetFields
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
