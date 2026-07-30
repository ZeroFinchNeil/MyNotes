using System.Security.Cryptography;
using System.Text.Json.Serialization;

using MyNotes.Constants;

namespace MyNotes.Models.Media;

[Debugging.Attributes.ReferenceTracker]
internal partial class ImageDescriptor : IEquatable<ImageDescriptor>
{
  public static ImageDescriptor Create(string originalFilePath)
  {
    byte[] randomBytes = new byte[16];
    RandomNumberGenerator.Fill(randomBytes);
    var fileName = System.IO.Path.ChangeExtension(Convert.ToHexStringLower(randomBytes), System.IO.Path.GetExtension(originalFilePath));
    return new() { FileName = fileName };
  }

  public ImageDescriptor() { TrackReference(); }

  public required string FileName { get; init; }

  [JsonIgnore]
  public string FilePath => System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, AppStrings.ImageFolderName, FileName);

  public bool Equals(ImageDescriptor? other) => other is not null && this.FileName == other.FileName;

  public override bool Equals(object? other) => Equals(other);

  public override int GetHashCode() => FileName.GetHashCode();

  public static bool operator ==(ImageDescriptor id1, ImageDescriptor id2) => id1.Equals(id2);

  public static bool operator !=(ImageDescriptor id1, ImageDescriptor id2) => !(id1 == id2);
}
