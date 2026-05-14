using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Queries.Navigations;

internal sealed record FindUserNavigationsAppQuery
{
  public NavigationId? Id { get; init; }
}
