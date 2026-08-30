using MyNotes.Constants;
using MyNotes.Domain.Media;

namespace MyNotes.Models.Media;

[Debugging.Attributes.ReferenceTracker]
internal partial class ImageDescriptor
{
  public required ImageId Id { get; init; }

  public required ImageCollectionKey ParentKey { get; init; }

  public string LocalFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, AppStrings.ImageFolderName, Path.ChangeExtension(Id.Name, StoredExtension));

  public required string OriginalFileName { get; init; }

  public required string StoredExtension { get; init; }

  public ImageDescriptor() { TrackReference(); }
}
