using Microsoft.Extensions.DependencyInjection;

using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.Views.Media;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerPage : Page
{
  private readonly ImageCollectionViewModel ViewModel;

  public ImageViewerPage(ImageCollectionKey imageCollectionKey)
  {
    TrackReference();
    InitializeComponent();
    var provider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    ViewModel = provider.Resolve(imageCollectionKey);
  }
}
