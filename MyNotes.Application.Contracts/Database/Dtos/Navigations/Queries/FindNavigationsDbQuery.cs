using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Contracts.Database.Dtos.Navigations.Queries;

internal record FindNavigationsDbQuery
{
  public NavigationId? Id { get; init; }
}
