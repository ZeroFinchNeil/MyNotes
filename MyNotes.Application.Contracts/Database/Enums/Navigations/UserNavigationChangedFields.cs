using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationChangedFields
{
  None,
  Parent,
  IsComposite,
  Icon,
  Title,
  IsDeleted,
}