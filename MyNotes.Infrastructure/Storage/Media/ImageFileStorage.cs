using System;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Media.Persistence;

using Windows.Storage;

namespace MyNotes.Infrastructure.Storage.Media;

internal sealed class ImageFileStorage : IImageFileStorage
{

  public async Task Save(string originalPath, string fileName, CancellationToken cancellationToken = default)
  {
    var originalFile = await StorageFile.GetFileFromPathAsync(originalPath);
    var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
    var copiedFile = await originalFile.CopyAsync(folder, fileName, NameCollisionOption.ReplaceExisting);
  }
}