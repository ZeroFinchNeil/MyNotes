using System;

namespace MyNotes.Application.Contracts.Database.Enums.Notes;

[Flags]
internal enum NoteFindFields
{
  None,
  NoteIdCondition,
  ParentIdCondition,
  TitleConditions,
  CreatedConditions,
  ModifiedConditions,
  BackgroundColorConditions,
  BookmarkedCondition,
  DeletedCondition
}