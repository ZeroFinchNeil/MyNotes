using System.Diagnostics.CodeAnalysis;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Models.Media;

namespace MyNotes.Common.Mappers;

internal static class NoteImageMapper
{
  public static ImageDescriptor ToModel(NoteImageDto imageDto) => new()
  {
    Id = imageDto.Id,
    CollectionKey = new ImageCollectionKey(imageDto.NoteId.Value),
    OriginalFileName = imageDto.OriginalFileName,
    StoredExtension = imageDto.StoredExtension
  };

  private static readonly IReadOnlyDictionary<string, string> ImageContentTypesByExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
  {
    [".jpg"] = "image/jpeg",
    [".jpeg"] = "image/jpeg",
    [".jpe"] = "image/jpeg",
    [".png"] = "image/png",
    [".apng"] = "image/apng",
    [".gif"] = "image/gif",
    [".bmp"] = "image/bmp",
    [".tif"] = "image/tiff",
    [".tiff"] = "image/tiff",
    [".webp"] = "image/webp",
    [".svg"] = "image/svg+xml",
    [".avif"] = "image/avif",
    [".heic"] = "image/heic",
    [".heif"] = "image/heif",
    [".ico"] = "image/vnd.microsoft.icon",
    [".jxl"] = "image/jxl",
  };

  public static bool TryGetContentType(string extension, [NotNullWhen(true)] out string? contentType)
  {
    contentType = null;

    if (string.IsNullOrWhiteSpace(extension))
    {
      return false;
    }

    string normalizedExtension = extension.Trim();

    if (!normalizedExtension.StartsWith('.'))
    {
      normalizedExtension = $".{normalizedExtension}";
    }

    return ImageContentTypesByExtension.TryGetValue(normalizedExtension, out contentType);
  }
}