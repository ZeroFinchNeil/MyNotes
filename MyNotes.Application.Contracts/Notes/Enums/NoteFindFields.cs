using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteFindFields
{
  None = 0,
  NoteIdCondition = 1 << 0,
  ParentIdCondition = 1 << 1,
  TitleConditions = 1 << 2,
  CreatedConditions = 1 << 3,
  ModifiedConditions = 1 << 4,
  BackgroundColorConditions = 1 << 5,
  BookmarkedCondition = 1 << 6,
  DeletedCondition = 1 << 7
}