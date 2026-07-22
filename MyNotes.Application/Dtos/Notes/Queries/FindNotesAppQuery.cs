using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Common.Querying;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Dtos.Notes.Queries;

internal sealed record FindNotesAppQuery
{
  public required NoteFindFields FindFields { get; init; }

  public AggregationMode AggregationMode { get; init; } = AggregationMode.All;

  public EqualityQueryCondition<NoteId>? NoteIdCondition { get; init; }

  public EqualityQueryCondition<NavigationId>? ParentIdCondition { get; init; }

  public QueryConditionSet<StringQueryCondition>? TitleConditions { get; init; }

  public QueryConditionSet<ComparisonQueryCondition<DateTimeOffset>>? CreatedConditions { get; init; }

  public QueryConditionSet<ComparisonQueryCondition<DateTimeOffset>>? ModifiedConditions { get; init; }

  public string? BackgroundColorConditions { get; init; }

  public EqualityQueryCondition<bool>? BookmarkedCondition { get; init; }

  public EqualityQueryCondition<bool>? DeletedCondition { get; init; }

  public void ThrowIfInvalid()
  {
    if (FindFields.HasFlag(NoteFindFields.NoteIdCondition) && NoteIdCondition is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.ParentIdCondition) && ParentIdCondition is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.TitleConditions) && TitleConditions is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.CreatedConditions) && CreatedConditions is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.ModifiedConditions) && ModifiedConditions is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.BackgroundColorConditions) && BackgroundColorConditions is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.BookmarkedCondition) && BookmarkedCondition is null)
    {
      throw new ArgumentException("");
    }
    if (FindFields.HasFlag(NoteFindFields.DeletedCondition) && DeletedCondition is null)
    {
      throw new ArgumentException("");
    }
  }
}