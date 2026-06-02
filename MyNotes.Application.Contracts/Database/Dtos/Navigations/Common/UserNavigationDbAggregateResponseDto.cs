using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record UserNavigationDbAggregateResponseDto
{
  public required UserNavigationDbResponseDto UserNavigationDbResponseDto { get; init; }

  public required UserNavigationViewStateDbResponseDto UserNavigationViewStateDbResponseDto { get; init; }
}
