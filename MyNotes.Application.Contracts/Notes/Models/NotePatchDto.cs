using DotNext;

using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Models;

internal sealed record NotePatchDto
{
  public required NoteId Id { get; init; }

  public Optional<NavigationId?> NavigationId { get; init; }

  public Optional<string> Title { get; init; }

  public Optional<byte[]> Body { get; init; }

  public Optional<string> BackgroundColor { get; init; }

  public Optional<string?> BackgroundImagePath { get; init; }

  public Optional<bool> IsBookmarked { get; init; }

  public Optional<bool> IsDeleted { get; init; }

  public bool IsEmpty => this is
  {
    NavigationId.IsUndefined: true,
    Title.IsUndefined: true,
    Body.IsUndefined: true,
    BackgroundColor.IsUndefined: true,
    BackgroundImagePath.IsUndefined: true,
    IsBookmarked.IsUndefined: true,
    IsDeleted.IsUndefined: true
  };
}