using System;

namespace MyNotes.Application.Contracts.Enums.Navigations;

[Flags]
internal enum CompositeNavigationViewStateUpdateFields
{
  None = 0,
  IsExpanded = 1 << 0,
}