using System;

namespace MyNotes.Application.Contracts.Database.Enums.Navigations;

[Flags]
internal enum UserCompositeNavigationViewStateGetFields
{
  None = 0,
  IsExpanded = 1 << 0,
}
