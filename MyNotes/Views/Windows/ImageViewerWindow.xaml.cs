using MyNotes.ViewModels.Media.Providers;
using MyNotes.Views.Media;

namespace MyNotes.Views.Windows;

[Debugging.ReferenceTracker]
internal sealed partial class ImageViewerWindow : Window
{
  #region Object Lifetime Management

  public ImageViewerWindow(ImageCollectionKey imageCollectionKey)
  {
    TrackReference();
    InitializeComponent();

    this.Content = new ImageViewerPage(imageCollectionKey);
    this.Closed += ImageViewerWindow_Closed;
  }
  public bool IsClosed { get; private set; }

  private void ImageViewerWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;
  }
  #endregion
}
