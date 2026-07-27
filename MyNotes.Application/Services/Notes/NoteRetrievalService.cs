using MyNotes.Application.Contracts.Enums.Notes;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Contracts.Models.Notes.Queries;
using MyNotes.Application.Contracts.Persistence.Notes;
using MyNotes.Common.Querying;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Services.Notes;

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

  public async Task<IReadOnlyList<NoteDto>> SearchNotesAsync(NoteFilterDto noteFilterDto, CancellationToken cancellationToken = default)
  {
    //List<NoteBundleAppResponseDto> noteDtos = new();
    //NoteSearcher.
    //return noteDtos.AsReadOnly();
    return [];
  }

  public async Task<IReadOnlyList<NoteDto>> GetOpenNotesAsync(CancellationToken cancellationToken = default)
  {
    return [];
  }
}