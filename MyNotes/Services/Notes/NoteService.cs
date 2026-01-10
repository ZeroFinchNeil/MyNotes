using CommunityToolkit.WinUI.Helpers;

using Microsoft.EntityFrameworkCore;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Database;
using MyNotes.Services.Database.Entities;

namespace MyNotes.Services.Notes;

internal sealed partial class NoteService : IDisposable
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public NoteService(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;

    SetCommands();
  }

  private bool _disposed;
  public void Dispose()
  {
    if (_disposed)
      return;

    _disposed = true;
  }
}

internal sealed partial class NoteService : IDisposable
{
  // Navigation 리스트에 해당하는 Note를 DB에서 가져오기
  public async Task<IReadOnlyList<Note>> GetNotesAsync(NavigationUserLeafNode navigation)
  {
    List<Note> notes;
    await using (var context = await DbContextFactory.CreateDbContextAsync())
    {
      notes = [.. context.NoteEntities
        .Where(e => e.Parent == navigation.Id.Value)
        .Select(e => new Note()
        {
          Id = NoteId.Create(e.Id),
          NavigationId = navigation.Id,
          Created = e.Created,
          Title = e.Title,
          Body = e.Body,
          Background = e.Background.ToColor(),
          Backdrop = (BackdropKind)e.Backdrop,
          Size = new SizeInt32(e.Width, e.Height),
          Position = new PointInt32(e.PositionX, e.PositionY),
          IsBookmarked = e.IsBookmarked,
          IsDeleted = e.IsDeleted
        })];
    }
    return notes;
  }

  // Note를 DB에 반영
  public async Task<Note?> AddNoteAsync(NavigationUserLeafNode navigation)
  {
    Note note = new()
    {
      Id = NoteId.NewId(),
      NavigationId = navigation.Id,
      Created = DateTimeOffset.UtcNow,
    };

    await using (var context = await DbContextFactory.CreateDbContextAsync())
    {
      if (!await context.NoteEntities.AnyAsync(e => e.Id == note.Id.Value))
      {
        NoteEntity entity = new()
        {
          Id = note.Id.Value,
          Parent = note.NavigationId.Value,
          Created = note.Created,
          Modified = note.Modified,
          Title = note.Title,
          Body = note.Body,
          Background = note.Background.ToString(),
          Backdrop = (int)note.Backdrop,
          Width = note.Size.Width,
          Height = note.Size.Height,
          PositionX = note.Position.X,
          PositionY = note.Position.Y,
          IsBookmarked = note.IsBookmarked,
          IsDeleted = note.IsDeleted
        };
        await context.NoteEntities.AddAsync(entity);
        await context.SaveChangesAsync().ConfigureAwait(true);
        return note;
      }
    }

    return null;
  }
}

internal sealed partial class NoteService : IDisposable
{
  private void SetCommands()
  {

  }
}