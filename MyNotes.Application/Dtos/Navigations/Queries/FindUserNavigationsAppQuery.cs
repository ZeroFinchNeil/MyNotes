using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Queries;

internal sealed record FindUserNavigationsAppQuery
{
  public NavigationId? Id { get; init; }
}
