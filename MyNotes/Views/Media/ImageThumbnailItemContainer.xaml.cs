using MyNotes.Constants;
using MyNotes.ViewModels.Media;

using Windows.Storage.FileProperties;

namespace MyNotes.Views.Media;

internal sealed partial class ImageThumbnailItemContainer : UserControl
{
  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ImageViewModel), typeof(ImageThumbnailItemContainer), new PropertyMetadata(null));
  public ImageViewModel ViewModel
  {
    get => (ImageViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register("DecodePixelWidth", typeof(int), typeof(ImageThumbnailItemContainer), new PropertyMetadata(null));
  public int DecodePixelWidth
  {
    get => (int)GetValue(DecodePixelWidthProperty);
    set => SetValue(DecodePixelWidthProperty, value);
  }

  public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register("DecodePixelHeight", typeof(int), typeof(ImageThumbnailItemContainer), new PropertyMetadata(null));
  public int DecodePixelHeight
  {
    get => (int)GetValue(DecodePixelHeightProperty);
    set => SetValue(DecodePixelHeightProperty, value);
  }

  public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageThumbnailItemContainer), new PropertyMetadata(Stretch.Uniform));
  public Stretch Stretch
  {
    get => (Stretch)GetValue(StretchProperty);
    set => SetValue(StretchProperty, value);
  }

  public ImageThumbnailItemContainer()
  {
    InitializeComponent();
    this.Loaded += ImageThumbnailItemContainer_Loaded;
    this.Unloaded += ImageThumbnailItemContainer_Unloaded;
  }
  private async void ImageThumbnailItemContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
    await SetThumbnailAsync();
    ViewModel.ImageChanged += ViewModel_ImageChanged;
  }

  private async void ViewModel_ImageChanged(object? sender, ImageChangedEventArgs e)
  {
    switch(e.Kind)
    {
      case ImageChangeKind.Modified:
      case ImageChangeKind.Replaced:
        await SetThumbnailAsync();
        break;
      default:
        await SetPlaceholderAsync();
        break;
    }
  }

  public async Task SetThumbnailAsync()
  {
    try
    {
      var file = await StorageFile.GetFileFromPathAsync(ViewModel.ImageDescriptor.LocalImageFilePath);
      var thumb = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)Math.Max(DecodePixelWidth, DecodePixelHeight), ThumbnailOptions.ResizeThumbnail);
      await ImageThumbnailItemContainer_BitmapImage.SetSourceAsync(thumb);
    }
    catch
    {
      await SetPlaceholderAsync();
    }
  }

  private async Task SetPlaceholderAsync()
  {
    try
    {
      var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(AppStrings.FallbackImagePath));
      await using var stream = await file.OpenStreamForReadAsync();
      await ImageThumbnailItemContainer_BitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
    }
    catch
    {
      ImageThumbnailItemContainer_BitmapImage = null;
    }
  }

  private void ImageThumbnailItemContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
    ViewModel.ImageChanged -= ViewModel_ImageChanged;
  }
}
