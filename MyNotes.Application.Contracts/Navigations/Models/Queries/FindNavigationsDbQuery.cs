using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Navigations.Models.Queries;

internal record FindNavigationsDbQuery
{
  public NavigationId? Id { get; init; }
}
