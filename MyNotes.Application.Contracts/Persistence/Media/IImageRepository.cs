using System.Threading.Tasks;

using MyNotes.Application.Contracts.Models.Media;

namespace MyNotes.Application.Contracts.Persistence.Media;

internal interface IImageRepository
{
  public Task AttachImageAsync(ImageDto imageDto);
}