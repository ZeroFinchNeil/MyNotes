using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteGetFields
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
