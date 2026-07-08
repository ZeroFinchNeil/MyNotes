using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationChangedFields
{
  None = 0,
  Parent = 1 << 0,
  Icon = 1 << 1,
  Title = 1 << 2,
  IsDeleted = 1 << 3,
}