using System;

namespace MyNotes.Application.Contracts.Navigations.Models;

[Flags]
internal enum CompositeNavigationViewStateProjectionFields
{
  None = 0,
  IsExpanded = 1 << 0,
}
