using System;

using MyNotes.Application.Contracts.Querying.Conditions;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Common.Querying.Conditions;

namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed record NoteFilterDto
{
  public required NoteFindFields NoteFindFields { get; init; }

  public AggregationMode AggregationMode { get; init; } = AggregationMode.All;

  public EqualityQueryCondition<Guid>? NoteIdCondition { get; init; }

  public EqualityQueryCondition<Guid>? ParentIdCondition { get; init; }

  public QueryConditionSet<StringQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<ComparisonQueryCondition<DateTimeOffset>>? CreatedConditions { get; init; }

  public QueryConditionSet<ComparisonQueryCondition<DateTimeOffset>>? ModifiedConditions { get; init; }

  public string? BackgroundColorConditions { get; init; }

  public EqualityQueryCondition<bool>? BookmarkedCondition { get; init; }

  public EqualityQueryCondition<bool>? DeletedCondition { get; init; }

  public void ThrowIfInvalid()
  {
    if (NoteFindFields.HasFlag(NoteFindFields.NoteIdCondition) && NoteIdCondition is null)
    {
      throw new InvalidOperationException(nameof(NoteIdCondition));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.ParentIdCondition) && ParentIdCondition is null)
    {
      throw new InvalidOperationException(nameof(ParentIdCondition));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.TitleConditions) && TitleConditions is null)
    {
      throw new InvalidOperationException(nameof(TitleConditions));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.CreatedConditions) && CreatedConditions is null)
    {
      throw new InvalidOperationException(nameof(CreatedConditions));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.ModifiedConditions) && ModifiedConditions is null)
    {
      throw new InvalidOperationException(nameof(ModifiedConditions));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.BackgroundColorConditions) && BackgroundColorConditions is null)
    {
      throw new InvalidOperationException(nameof(BackgroundColorConditions));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.BookmarkedCondition) && BookmarkedCondition is null)
    {
      throw new InvalidOperationException(nameof(BookmarkedCondition));
    }
    if (NoteFindFields.HasFlag(NoteFindFields.DeletedCondition) && DeletedCondition is null)
    {
      throw new InvalidOperationException(nameof(DeletedCondition));
    }
  }
}