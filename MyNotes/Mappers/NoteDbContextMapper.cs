using System.Text.Json;

using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Models.Media;
using MyNotes.Models.Notes;
using MyNotes.Shared.Constants;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Mappers;

[Obsolete]
internal static class NoteDbContextMapper
{
#if false
  private static readonly ImmutableDictionary<string, Func<NoteModel, Action<NoteEntity>>> _notePropertyToEntityActions = ImmutableDictionary.CreateRange(new Dictionary<string, Func<NoteModel, Action<NoteEntity>>>()
  {
    { nameof(NoteModel.NavigationId), note => e => e.Parent = note.NavigationId.Value },
    { nameof(NoteModel.Modified), note => e => e.Modified = note.Modified },
    { nameof(NoteModel.Title), note => e => e.Title = note.Title },
    { nameof(NoteModel.Body), note => e => e.Body = note.Body },
    { nameof(NoteModel.BackgroundColor), note => e => e.BackgroundColor = note.BackgroundColor.ToString() },
    { nameof(NoteModel.ShowBackgroundImage), note => e => e.ShowBackgroundImage = note.ShowBackgroundImage },
    { nameof(NoteModel.BackgroundImagePath), note => e => e.BackgroundImagePath = note.BackgroundImagePath },
    { nameof(NoteModel.BackgroundImageOpacity), note => e => e.BackgroundImageOpacity = note.BackgroundImageOpacity },
    { nameof(NoteModel.BackgroundImageBlur), note => e => e.BackgroundImageBlur = note.BackgroundImageBlur },
    { nameof(NoteModel.BackdropKind), note => e => e.BackdropKind = (int)note.BackdropKind },
    { nameof(NoteModel.BackdropTintOpacity), note => e => e.BackdropTintOpacity = Math.Round(note.BackdropTintOpacity, 2) },
    { nameof(NoteModel.BackdropLuminosityOpacity), note => e => e.BackdropLuminosityOpacity = Math.Round(note.BackdropLuminosityOpacity, 2) },
    { nameof(NoteModel.Images), note => e => e.Images = JsonSerializer.Serialize(note.Images, AppJson.JsonSerializerOptions) },
    { nameof(NoteModel.ShowImagePanel), note => e => e.ShowImagePanel = note.ShowImagePanel },
    { nameof(NoteModel.ImagePanelHeight), note => e => e.ImagePanelHeight = Math.Round(note.ImagePanelHeight, 2) },
    { nameof(NoteModel.Size), note => e =>
      {
        e.Width = note.Size.Width;
        e.Height = note.Size.Height;
      }
    },
    { nameof(NoteModel.Position), note => e =>
      {
        e.PositionX = note.Position.X;
        e.PositionY = note.Position.Y;
      }
    },
    { nameof(NoteModel.IsBookmarked), note => e => e.IsBookmarked = note.IsBookmarked },
    { nameof(NoteModel.IsDeleted), note => e => e.IsDeleted = note.IsDeleted },
    { nameof(NoteModel.IsWindowOpen), note => e => e.IsWindowOpen = note.IsWindowOpen },
    { nameof(NoteModel.IsAlwaysOnTop), note => e => e.IsAlwaysOnTop = note.IsAlwaysOnTop }
  });
  private static readonly ImmutableHashSet<string> _notePropertyToNoteSearchEntity = [nameof(NoteModel.Title), nameof(NoteModel.BodyPlainText)];

  private static NoteModel EntityToNote(NoteEntity e)
  {
    NoteId noteId = NoteId.Create(e.Id);
    if (NoteCache.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var existingNote))
    {
      return existingNote;
    }

    List<ImageDescriptor>? images = null;
    try
    {
      images = JsonSerializer.Deserialize<List<ImageDescriptor>>(e.Images);
    }
    catch
    { }
    images ??= new();

    NoteModel note = new()
    {
      Id = noteId,
      NavigationId = NavigationId.GetOrCreate(e.Parent),
      Created = e.Created,
      Title = e.Title,
      Body = e.Body,
      BackgroundColor = e.BackgroundColor.ToColor(),
      ShowBackgroundImage = e.ShowBackgroundImage,
      BackgroundImagePath = e.BackgroundImagePath,
      BackgroundImageOpacity = e.BackgroundImageOpacity,
      BackgroundImageBlur = e.BackgroundImageBlur,
      BackdropKind = (BackdropKind)e.BackdropKind,
      BackdropTintOpacity = e.BackdropTintOpacity,
      BackdropLuminosityOpacity = e.BackdropLuminosityOpacity,
      Images = [.. images],
      ShowImagePanel = e.ShowImagePanel,
      ImagePanelHeight = e.ImagePanelHeight,
      Size = new SizeInt32(e.Width, e.Height),
      Position = new PointInt32(e.PositionX, e.PositionY),
      IsBookmarked = e.IsBookmarked,
      IsDeleted = e.IsDeleted,
      IsWindowOpen = e.IsWindowOpen,
      IsAlwaysOnTop = e.IsAlwaysOnTop
    };
    NoteCache[noteId] = new WeakReference<NoteModel>(note);
    return note;
  }

  public static NoteEntity NoteToEntity(NoteModel note) => new()
  {
    Id = note.Id.Value,
    Parent = note.NavigationId.Value,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    BackgroundColor = note.BackgroundColor.ToString(),
    ShowBackgroundImage = note.ShowBackgroundImage,
    BackgroundImagePath = note.BackgroundImagePath,
    BackgroundImageOpacity = note.BackgroundImageOpacity,
    BackgroundImageBlur = note.BackgroundImageBlur,
    BackdropKind = (int)note.BackdropKind,
    BackdropTintOpacity = note.BackdropTintOpacity,
    BackdropLuminosityOpacity = note.BackdropLuminosityOpacity,
    Images = JsonSerializer.Serialize(note.Images, AppJson.JsonSerializerOptions),
    ShowImagePanel = note.ShowImagePanel,
    ImagePanelHeight = note.ImagePanelHeight,
    Width = note.Size.Width,
    Height = note.Size.Height,
    PositionX = note.Position.X,
    PositionY = note.Position.Y,
    IsBookmarked = note.IsBookmarked,
    IsDeleted = note.IsDeleted,
    IsWindowOpen = note.IsWindowOpen,
    IsAlwaysOnTop = note.IsAlwaysOnTop
  };

  extension(NoteModel note)
  {
    public NoteEntity ToEntity() => NoteToEntity(note);
  }
#endif
}
