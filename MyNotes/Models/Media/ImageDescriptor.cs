using MyNotes.Constants;
using MyNotes.Domain.Media;

namespace MyNotes.Models.Media;

[Debugging.Attributes.ReferenceTracker]
internal partial class ImageDescriptor
{
  public required ImageId Id { get; init; }

  public string FilePath => System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, AppStrings.ImageFolderName, Id.Name);

  public ImageDescriptor() { TrackReference(); }
}
