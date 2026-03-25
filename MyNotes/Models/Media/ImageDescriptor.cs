using System.Text.Json.Serialization;

using MyNotes.AppConstants;

namespace MyNotes.Models.Media;

internal class ImageDescriptor : IEquatable<ImageDescriptor>
{
  public required string FileName { get; init; }

  [JsonIgnore]
  public string FilePath => System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, AppStrings.ImageFolderPath, FileName);

  public bool Equals(ImageDescriptor? other) => other is not null && this.FileName == other.FileName;

  public override bool Equals(object? other) => Equals(other);

  public override int GetHashCode() => FileName.GetHashCode();

  public static bool operator ==(ImageDescriptor id1, ImageDescriptor id2) => id1.Equals(id2);

  public static bool operator !=(ImageDescriptor id1, ImageDescriptor id2) => !(id1 == id2);
}
