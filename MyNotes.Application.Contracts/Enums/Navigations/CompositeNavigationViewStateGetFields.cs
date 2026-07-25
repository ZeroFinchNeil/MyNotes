using System;

namespace MyNotes.Application.Contracts.Enums.Navigations;

[Flags]
internal enum CompositeNavigationViewStateGetFields
{
  None = 0,
  IsExpanded = 1 << 0,
}
