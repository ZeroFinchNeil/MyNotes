using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Queries;

internal sealed record FindNavigationsAppQuery
{
  public NavigationId? Id { get; init; }
}
