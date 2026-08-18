using System.Runtime.CompilerServices;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Application.Contracts.Querying.Conditions;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Notes.Services;

internal sealed partial class NoteRetrievalService
{
  private readonly INoteRepository NoteRepository;
  private readonly INoteSearcher NoteSearcher;

  public NoteRetrievalService(INoteRepository noteRepository, INoteSearcher noteSearcher)
  {
    NoteRepository = noteRepository;
    NoteSearcher = noteSearcher;
  }

  public async Task<NoteDto?> GetNoteByIdAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    return await NoteRepository.GetNoteByIdAsync(noteId, cancellationToken);
  }

  public async Task<IReadOnlyList<NoteDto>> GetNotesByParentAsync(NavigationId parentId, bool includeDeleted = false, CancellationToken cancellationToken = default)
  {
    return await NoteRepository.GetNotesByParentAsync(parentId, includeDeleted, cancellationToken);
  }

  public async Task<IReadOnlyList<NoteDto>> GetBookmarkedNotesAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
  {
    NoteFilterDto noteFilterDto = includeDeleted
      ? new()
      {
        NoteFindFields = NoteFindFields.BookmarkedCondition,
        BookmarkedCondition = EqualityQueryCondition<bool>.Create(true, EqualityMatchType.Equals),
      }
      : new()
      {
        NoteFindFields = NoteFindFields.BookmarkedCondition | NoteFindFields.DeletedCondition,
        BookmarkedCondition = EqualityQueryCondition<bool>.Create(true, EqualityMatchType.Equals),
        DeletedCondition = EqualityQueryCondition<bool>.Create(false, EqualityMatchType.Equals)
      };
    return await FindNotesAsync(noteFilterDto, cancellationToken);
  }

  public async Task<IReadOnlyList<NoteDto>> GetTrashedNotesAsync(CancellationToken cancellationToken = default)
  {
    NoteFilterDto noteFilterDto = new()
    {
      NoteFindFields = NoteFindFields.DeletedCondition,
      DeletedCondition = EqualityQueryCondition<bool>.Create(true, EqualityMatchType.Equals)
    };
    return await FindNotesAsync(noteFilterDto, cancellationToken);
  }

  public async Task<IReadOnlyList<NoteDto>> FindNotesAsync(NoteFilterDto noteFilterDto, CancellationToken cancellationToken = default)
  {
    return await NoteRepository.FindNotesAsync(noteFilterDto, cancellationToken);
  }

  private readonly int _batchSize = 50;
  public async IAsyncEnumerable<NoteSearchResultDto> SearchNotesAsync(string searchText, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    Dictionary<NoteId, NoteSearchHitDto> hitBuffer = new(_batchSize);
    await foreach (var hitDto in NoteSearcher.GetNotesAsync(searchText, cancellationToken))
    {
      hitBuffer.Add(hitDto.NoteId, hitDto);
      if (hitBuffer.Count < _batchSize)
      {
        continue;
      }

      foreach (var noteDto in await NoteRepository.GetNotesByIdsAsync(hitBuffer.Keys, cancellationToken))
      {
        if (hitBuffer.TryGetValue(noteDto.Id, out var matchedHit))
        {
          yield return new() { NoteDto = noteDto, HitDto = matchedHit };
        }
      }
      hitBuffer.Clear();
    }

    if (hitBuffer.Count > 0)
    {
      foreach (var noteDto in await NoteRepository.GetNotesByIdsAsync(hitBuffer.Keys, cancellationToken))
      {
        if (hitBuffer.TryGetValue(noteDto.Id, out var matchedHit))
        {
          yield return new() { NoteDto = noteDto, HitDto = matchedHit };
        }
      }
    }
  }

  public async Task<IReadOnlyList<NoteDto>> GetOpenNotesAsync(CancellationToken cancellationToken = default)
  {
    return [];
  }
}