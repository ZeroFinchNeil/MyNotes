using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.Notes;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.Views.Media;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerPage : Page
{
  private readonly ImageCollectionViewModelProvider viewModelProvider;
  private readonly ImageCollectionViewModel ViewModel;
  public NoteId NoteId { get; }

  public ImageViewerPage(NoteId noteId)
  {
    TrackReference();
    InitializeComponent();

    NoteId = noteId;
    viewModelProvider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    ViewModel = viewModelProvider.Resolve(NoteId);

    this.Unloaded += ImageViewerPage_Unloaded;
  }

  private void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
  {
    viewModelProvider.Release(NoteId);
  }
}
