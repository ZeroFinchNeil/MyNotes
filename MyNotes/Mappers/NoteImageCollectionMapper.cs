using MyNotes.Models.Notes;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.Mappers;

internal static class NoteImageCollectionMapper
{
  public static ImageCollectionKey CreateImageCollectionKey(NoteModel note) => new()
  {
    Key = note.Id.Value,
    CollectionReference = new WeakReference<ObservableCollection<ImageViewModel>>([.. note.Images.Select(ImageViewModelProvider.Resolve)])
  };
}
