using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.Notes;
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
  public NoteId NoteId { get; }

  public ImageViewerPage(NoteId noteId)
  {
    TrackReference();
    InitializeComponent();

    NoteId = noteId;
    viewModelProvider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    _viewmodelLease = viewModelProvider.Resolve(NoteId);

    this.Unloaded += ImageViewerPage_Unloaded;
  }

  private void ImageViewerPage_Unloaded(object sender, RoutedEventArgs e)
  {
    _viewmodelLease.Dispose();
  }
}
