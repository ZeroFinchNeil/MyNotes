using MyNotes.Common.Commands;
using MyNotes.ViewModels.Media;

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
  private void ImageThumbnailItemContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void ImageThumbnailItemContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}
