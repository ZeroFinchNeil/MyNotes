using System;

namespace MyNotes.Application.Contracts.Navigations.Enums;

[Flags]
internal enum CompositeNavigationViewStateUpdateFields
{
  None = 0,
  IsExpanded = 1 << 0,
}