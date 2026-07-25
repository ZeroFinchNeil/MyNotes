using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Models.Navigations.Queries;

internal record FindNavigationsDbQuery
{
  public NavigationId? Id { get; init; }
}
