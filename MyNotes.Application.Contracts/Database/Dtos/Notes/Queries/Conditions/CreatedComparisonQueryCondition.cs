using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries.Enums;
using MyNotes.Application.Contracts.Query;

namespace MyNotes.Application.Contracts.Database.Dtos.Notes.Queries.Conditions;

internal class CreatedComparisonQueryCondition : CreatedQueryCondition, IQueryCondition<ComparisonOperator>
{
  public required ComparisonOperator Condition { get; init; }
}