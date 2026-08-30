using CommunityToolkit.Mvvm.Messaging;

using MyNotes.Constants;
using MyNotes.Models.Media;
using MyNotes.Strings;
using MyNotes.Views.Media;

namespace MyNotes.Views.Windows;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerWindow : Window
{
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

    this.Content = new ImageViewerPage(collectionKey);

    RegisterMessengers();
    this.Closed += ImageViewerWindow_Closed;
  }
  public bool IsClosed { get; private set; }

  private void ImageViewerWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;
    UnregisterMessengers();
  }
  #endregion

  private void RegisterMessengers()
  {
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
