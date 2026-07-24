using System;

namespace MyNotes.Application.Contracts.Navigations.Enums;

[Flags]
internal enum CompositeNavigationViewStateGetFields
{
  None = 0,
  IsExpanded = 1 << 0,
}
