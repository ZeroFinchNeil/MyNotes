using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Media.Persistence;

using Windows.Storage;

namespace MyNotes.Infrastructure.Storage.Media;

internal sealed class ImageFileStorage : IImageFileStorage
{
  private static string _imageFolderName = "Images";

  public async Task SaveImage(string originalPath, string fileName, CancellationToken cancellationToken = default)
  {
    var originalFile = await StorageFile.GetFileFromPathAsync(originalPath);
    var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_imageFolderName, CreationCollisionOption.OpenIfExists);
    var copiedFile = await originalFile.CopyAsync(folder, fileName, NameCollisionOption.ReplaceExisting);
  }

  public async Task DeleteImage(string fileNameWithoutExtension, CancellationToken cancellationToken = default)
  {
    try
    {
      if (await ApplicationData.Current.LocalFolder.CreateFolderAsync(_imageFolderName, CreationCollisionOption.OpenIfExists) is StorageFolder folder)
      {
        foreach (var file in await folder.GetFilesAsync())
        {
          if (Path.GetFileNameWithoutExtension(file.Name).Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
          {
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
          }
        }
      }
    }
    catch (FileNotFoundException)
    {

    }
    catch
    {

    }
  }
}