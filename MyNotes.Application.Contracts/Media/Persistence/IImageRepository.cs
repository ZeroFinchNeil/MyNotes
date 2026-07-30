using System.Threading.Tasks;

using MyNotes.Application.Contracts.Media.Models;

namespace MyNotes.Application.Contracts.Media.Persistence;

internal interface IImageRepository
{
  public Task AttachImageAsync(ImageDto imageDto);
}