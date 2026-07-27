using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.ValueObjects;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.Views.Media;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerPage : Page
{
  private readonly ImageCollectionViewModel ViewModel;

  public ImageViewerPage(NoteId noteId)
  {
    TrackReference();
    InitializeComponent();
    var provider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    ViewModel = provider.Resolve(noteId);
  }
}
