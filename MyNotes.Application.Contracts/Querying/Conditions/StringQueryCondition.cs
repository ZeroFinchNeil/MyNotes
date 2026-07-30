using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Application.Contracts.Querying.Conditions;

internal class StringQueryCondition : IQueryCondition<string, TextMatchType, StringQueryCondition>
{
  public static StringQueryCondition Create(string target, TextMatchType condition) => new() { Target = target, Condition = condition };

  public required string Target { get; init; }

  public required TextMatchType Condition { get; init; }
}