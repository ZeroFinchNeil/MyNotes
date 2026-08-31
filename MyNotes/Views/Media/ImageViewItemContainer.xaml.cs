using MyNotes.Common.Commands;
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

  public static readonly DependencyProperty IsOpenInNewWindowAvailableProperty = DependencyProperty.Register("IsOpenInNewWindowAvailable", typeof(bool), typeof(ImageViewItemContainer), new PropertyMetadata(true, OnIsOpenInNewWindowAvailableChanged));
  public bool IsOpenInNewWindowAvailable
  {
    get => (bool)GetValue(IsOpenInNewWindowAvailableProperty);
    set => SetValue(IsOpenInNewWindowAvailableProperty, value);
  }

  private static void OnIsOpenInNewWindowAvailableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if(d is ImageViewItemContainer container)
    {
      container._showImageCommand.NotifyCanExecuteChanged();
    }
  }

  public ImageViewItemContainer()
  {
    InitializeComponent();
    this.Loaded += ImageViewItemContainer_Loaded;
    this.Unloaded += ImageViewItemContainer_Unloaded;

    _showImageCommand = new()
    {
      ExecuteFunc = async () => await ViewModel.ShowImageCommand.ExecuteAsync(),
      CanExecuteFunc = () => IsOpenInNewWindowAvailable
    };
  }
  private void ImageViewItemContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void ImageViewItemContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }

  private readonly AsyncCommand _showImageCommand;
}
