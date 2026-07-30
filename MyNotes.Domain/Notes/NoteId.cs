using System;

namespace MyNotes.Domain.Notes;

internal readonly record struct NoteId
{
  public static NoteId NewId() => new(Guid.NewGuid());

  public static NoteId Create(Guid id) => new(id);
  public static NoteId Create(string id) => Create(Guid.Parse(id));

  public Guid Value { get; init; }

  private NoteId(Guid id) => Value = id;
  public NoteId() => throw new InvalidOperationException("NoteId has not been properly initialized.");
}
