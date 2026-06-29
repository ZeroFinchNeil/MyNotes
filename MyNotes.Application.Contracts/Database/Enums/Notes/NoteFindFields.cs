using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteFindFields
{
  None = 0,
  NoteIdCondition = 1,
  ParentIdCondition = 2,
  TitleConditions = 4,
  CreatedConditions = 8,
  ModifiedConditions = 16,
  BackgroundColorConditions = 32,
  BookmarkedCondition = 64,
  DeletedCondition = 128
}