using MyNotes.Application.Contracts.Database.Repositories.Navigations;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationRetrievalService
{
  private readonly INavigationRepository NavigationRepository;

  public NavigationRetrievalService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
  }
}
