using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserNavigationGetFields
{
  None = 0,
  Id = 1 << 0,
  Parent = 1 << 1,
  IsComposite = 1 << 2,
  Icon = 1 << 3,
  Title = 1 << 4,
  IsDeleted = 1 << 5,
}
