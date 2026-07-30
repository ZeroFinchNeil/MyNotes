using System;

namespace MyNotes.Application.Contracts.Navigations.Models;

[Flags]
internal enum NavigationProjectionFields
{
  None = 0,
  Id = 1 << 0,
  ParentId = 1 << 1,
  IsComposite = 1 << 2,
  Icon = 1 << 3,
  Title = 1 << 4,
  IsDeleted = 1 << 5,
}
