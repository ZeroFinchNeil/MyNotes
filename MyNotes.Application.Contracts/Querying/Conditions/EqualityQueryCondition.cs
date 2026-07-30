using MyNotes.Application.Contracts.Querying.Conditions;
using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Common.Querying.Conditions;

internal sealed class EqualityQueryCondition<TTarget> : IQueryCondition<TTarget, EqualityMatchType, EqualityQueryCondition<TTarget>> where TTarget : notnull
{
  public static EqualityQueryCondition<TTarget> Create(TTarget target, EqualityMatchType condition) => new() { Target = target, Condition = condition };

  public required TTarget Target { get; init; }

  public required EqualityMatchType Condition { get; init; }
}