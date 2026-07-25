using System;

namespace MyNotes.Application.Contracts.Enums.Notes;

[Flags]
internal enum NoteGetFields
{
  None = 0,
  Id = 1 << 0,
  NavigationId = 1 << 1,
  Created = 1 << 2,
  Modified = 1 << 3,
  Title = 1 << 4,
  Body = 1 << 5,
  BodyImagePaths = 1 << 6,
  BackgroundColor = 1 << 7,
  BackgroundImagePath = 1 << 8,
  IsBookmarked = 1 << 9,
  IsDeleted = 1 << 10
}
