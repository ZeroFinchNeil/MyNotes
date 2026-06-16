using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationViewStateGetFields
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
