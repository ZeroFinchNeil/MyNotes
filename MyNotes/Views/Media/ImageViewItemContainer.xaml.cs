using MyNotes.ViewModels.Media;

namespace MyNotes.Views.Media;

internal sealed partial class ImageViewItemContainer : UserControl
{
  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ImageViewModel), typeof(ImageViewItemContainer), new PropertyMetadata(null));
  public ImageViewModel ViewModel
  {
    get => (ImageViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageViewItemContainer), new PropertyMetadata(Stretch.Uniform));
  public Stretch Stretch
  {
    get => (Stretch)GetValue(StretchProperty);
    set => SetValue(StretchProperty, value);
  }

  public ImageViewItemContainer()
  {
    InitializeComponent();
    this.Loaded += ImageViewItemContainer_Loaded;
    this.Unloaded += ImageViewItemContainer_Unloaded;
  }
  private void ImageViewItemContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void ImageViewItemContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}