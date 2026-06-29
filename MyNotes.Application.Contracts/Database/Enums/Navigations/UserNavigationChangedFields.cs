using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationChangedFields
{
  None = 0,
  Parent = 1 << 0,
  IsComposite = 1 << 1,
  Icon = 1 << 2,
  Title = 1 << 3,
  IsDeleted = 1 << 4,
}