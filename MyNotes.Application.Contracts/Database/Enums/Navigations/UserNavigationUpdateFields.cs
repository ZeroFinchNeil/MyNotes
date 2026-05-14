using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationUpdateFields
{
  None,
  Parent,
  IsComposite,
  Icon,
  Title,
  IsDeleted,
}