using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Media;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;

namespace MyNotes.Views.Media;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerPage : Page
{
  private readonly ImageCollectionViewModelProvider viewModelProvider;

  private readonly IViewModelLease<ImageCollectionViewModel> _viewmodelLease;
  private ImageCollectionViewModel ViewModel => _viewmodelLease.ViewModel;
  public ImageCollectionKey CollectionKey { get; }

  public static readonly DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage", typeof(ImageViewModel), typeof(ImageViewerPage), new PropertyMetadata(null));
  public ImageViewModel? SelectedImage
  {
    get => (ImageViewModel?)GetValue(SelectedImageProperty);
    set => SetValue(SelectedImageProperty, value);
  }

  public ImageViewerPage(ImageCollectionKey collectionKey)
  {
    TrackReference();
    InitializeComponent();

    CollectionKey = collectionKey;
    viewModelProvider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    _viewmodelLease = viewModelProvider.Resolve(CollectionKey);

    this.Unloaded += ImageViewerPage_Unloaded;
  }

  public void ChangeImageSelection(ImageDescriptor imageDescriptor)
  {
    SelectedImage = ViewModel.ImageViewModels.FirstOrDefault(vm => vm.ImageDescriptor == imageDescriptor);
  }

  private void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel.ResetImagesCache();
    _viewmodelLease.Dispose();
  }
}
