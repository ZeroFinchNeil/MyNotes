namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;

internal sealed record UserCompositeNavigationViewStateDbResponseDto : UserNavigationViewStateDbResponseDto
{
  public required bool IsExpanded { get; init; }
}