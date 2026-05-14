using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationGetFields
{
  None,
  Id,
  Parent,
  IsComposite,
  Icon,
  Title,
  Position,
  IsDeleted
}
