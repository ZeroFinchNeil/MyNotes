using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal abstract record UserNavigationViewStateDbResponseDto
{
  public required Guid Id { get; init; }
}
