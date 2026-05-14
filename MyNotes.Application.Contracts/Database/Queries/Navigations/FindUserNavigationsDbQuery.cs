using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Queries.Navigations;

internal record FindUserNavigationsDbQuery
{
  public NavigationId? Id { get; init; }
}
