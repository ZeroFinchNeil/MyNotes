using MyNotes.Common.Interop;
using MyNotes.Constants;
using MyNotes.Models.Media;
using MyNotes.Strings;
using MyNotes.Views.Media;

namespace MyNotes.Views.Windows;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerWindow : Window
{
  private readonly IntPtr _hWnd;
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

    // hWnd(Window Handle) 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

    // DPI 스케일 가져오기
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);
    var minimumWindowSize = AppSettingsDescriptors.ImageViewerWindowMinimumSize;
    var presenter = AppWindow.Presenter as OverlappedPresenter;
    presenter?.PreferredMinimumWidth = (int)(minimumWindowSize.Width * scaleFactor);
    presenter?.PreferredMinimumHeight = (int)(minimumWindowSize.Height * scaleFactor);

    InitializationTask = InitializeAsync(collectionKey);

    this.Closed += ImageViewerWindow_Closed;
  }
  public bool IsClosed { get; private set; }

  public Task InitializationTask { get; }
  private async Task InitializeAsync(ImageCollectionKey collectionKey)
  {
    _content = new ImageViewerPage(collectionKey);
    await _content.InitializationTask;
    this.Content = _content;
    this.SetTitleBar(_content.TitleBarElement);
  }

  private async void ImageViewerWindow_Closed(object sender, WindowEventArgs args)
  {
    this.Closed -= ImageViewerWindow_Closed;
    if (_content is not null)
    {
      await _content.DisposeAsync();
      _content = null;
    }
    IsClosed = true;
  }
  #endregion

  public async Task ChangeImageSelection(ImageDescriptor? imageDescriptor)
  {
    if (imageDescriptor is not null && _content is not null)
    {
      await _content.ChangeImageSelection(imageDescriptor);
    }
  }
}
