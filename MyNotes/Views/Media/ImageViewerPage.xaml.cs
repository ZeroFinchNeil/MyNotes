using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Media;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.Views.Media;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerPage : Page, ITitleBarProvider, IAsyncDisposable
{
  public UIElement TitleBarElement { get; }

  private IAsyncViewModelLease<ImageCollectionViewModel>? ViewModelLease;
  private ImageCollectionViewModel ViewModel => ViewModelLease?.ViewModel ?? throw new InvalidOperationException();
  public ImageCollectionKey CollectionKey { get; }

  public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage", typeof(ImageViewModel), typeof(ImageViewerPage), new PropertyMetadata(null));
  public ImageViewModel? SelectedImage
  {
    get => (ImageViewModel?)GetValue(SelectedImageProperty);
    set => SetValue(SelectedImageProperty, value);
  }

  public static readonly DependencyProperty IsAlwaysOnTopProperty = DependencyProperty.Register("IsAlwaysOnTop", typeof(bool), typeof(ImageViewerPage), new PropertyMetadata(false, OnIsAlwaysOnTopChanged));
  public bool IsAlwaysOnTop
  {
    get => (bool)GetValue(IsAlwaysOnTopProperty);
    set => SetValue(IsAlwaysOnTopProperty, value);
  }

  public static void OnIsAlwaysOnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is ImageViewerPage control && e.NewValue is bool newValue)
    {
      var appWindow = AppWindow.GetFromWindowId(control.XamlRoot.ContentIslandEnvironment.AppWindowId);
      if (appWindow.Presenter is OverlappedPresenter presenter)
      {
        presenter.IsAlwaysOnTop = newValue;
      }
    }
  }

  public ImageViewerPage(ImageCollectionKey collectionKey)
  {
    TrackReference();
    InitializeComponent();

    TitleBarElement = ImageViewerPage_TitleBar;
    CollectionKey = collectionKey;
    InitializationTask = InitializeAsync();
  }

  public Task InitializationTask { get; }
  private async Task InitializeAsync()
  {
    var viewModelProvider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    ViewModelLease = await viewModelProvider.ResolveAsync(CollectionKey);
  }

  private bool _disposeStarted;
  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore();
    GC.SuppressFinalize(this);
  }

  public async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    Bindings.StopTracking();
    ViewModel.ResetImagesCache();

    if (ViewModelLease is not null)
    {
      await ViewModelLease.DisposeAsync();
    }
  }

  public async Task ChangeImageSelection(ImageDescriptor imageDescriptor)
  {
    await ViewModel.InitializationTask;
    SelectedImage = ViewModel.ImageViewModels.FirstOrDefault(vm => vm.ImageDescriptor == imageDescriptor);
  }
}
