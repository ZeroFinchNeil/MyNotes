namespace MyNotes.Application.Contracts.Database.Dtos.Navigations;

internal sealed record CreateUserNavigationDbAggregateRequestDto
{
  public required CreateUserNavigationDbRequestDto CreateUserNavigationDbRequestDto { get; init; }

  public required CreateUserNavigationViewStateDbRequestDto CreateUserNavigationViewStateDbRequestDto { get; init; }
}
