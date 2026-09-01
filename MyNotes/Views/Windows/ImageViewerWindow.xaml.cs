using MyNotes.Constants;
using MyNotes.Models.Media;
using MyNotes.Strings;
using MyNotes.Views.Media;

namespace MyNotes.Views.Windows;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerWindow : Window
{
  private ImageViewerPage? _content;

  #region Object Lifetime Management
  public ImageViewerWindow(ImageCollectionKey collectionKey)
  {
    TrackReference();
    InitializeComponent();

    // 타이틀 및 아이콘 설정
    AppWindow.Title = LocalizedStrings.ImageViewerWindowTitle;
    this.ExtendsContentIntoTitleBar = true;

    AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
    AppWindow.SetIcon(AppStrings.AppIconPath);
    AppWindow.SetTaskbarIcon(AppStrings.AppIconPath);

    _content = new ImageViewerPage(collectionKey);
    this.Content = _content;

    this.Closed += ImageViewerWindow_Closed;
  }
  public bool IsClosed { get; private set; }

  private void ImageViewerWindow_Closed(object sender, WindowEventArgs args)
  {
    _content = null;
    this.Closed -= ImageViewerWindow_Closed;
    IsClosed = true;
  }
  #endregion

  public void ChangeImageSelection(ImageDescriptor? imageDescriptor)
  {
    if (imageDescriptor is not null)
    {
      _content?.ChangeImageSelection(imageDescriptor);
    }
  }
}
