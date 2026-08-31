using MyNotes.Constants;
using MyNotes.Domain.Media;

namespace MyNotes.Models.Media;

[Debugging.Attributes.ReferenceTracker]
internal partial class ImageDescriptor
{
  public required ImageId Id { get; init; }

  public required ImageCollectionKey CollectionKey { get; init; }

  public static string LocalImageFolderPath => AppStrings.ImageFolderPath;

  public string LocalImageFileName => Path.ChangeExtension(Id.Name, StoredExtension);

  public string LocalImageFilePath => Path.Combine(LocalImageFolderPath, LocalImageFileName);

  public required string OriginalFileName { get; init; }

  public required string StoredExtension { get; init; }

  public ImageDescriptor() { TrackReference(); }
}
