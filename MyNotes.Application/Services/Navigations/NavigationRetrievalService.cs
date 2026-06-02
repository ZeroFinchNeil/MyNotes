using MyNotes.Application.Contracts.Database.Queries.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Mappers;
using MyNotes.Application.Queries.Navigations;

namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationRetrievalService
{
  private readonly INavigationRepository NavigationRepository;
  public NavigationRetrievalService(INavigationRepository navigationRepository)
  {
    NavigationRepository = navigationRepository;
  }
}
