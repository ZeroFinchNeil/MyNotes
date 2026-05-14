using MyNotes.Application.Contracts.Database.Queries.Notes.Enums;
using MyNotes.Application.Contracts.Queries;

namespace MyNotes.Application.Contracts.Database.Queries.Notes.Conditions;

internal class CreatedComparisonQueryCondition : CreatedQueryCondition, IQueryCondition<ComparisonOperator>
{
  public required ComparisonOperator Condition { get; init; }
}