namespace MyNotes.Application.Navigations.Services;

internal sealed partial class NavigationService
{
  public NavigationCreationService Creation { get; }
  public NavigationRetrievalService Retrieval { get; }
  public NavigationModificationService Modification { get; }
  public NavigationArrangementService Arrangement { get; }

  public NavigationService(NavigationCreationService navigationCreationService, NavigationRetrievalService navigationRetrievalService, NavigationModificationService navigationModificationService, NavigationArrangementService navigationArrangementService)
  {
    Creation = navigationCreationService;
    Retrieval = navigationRetrievalService;
    Modification = navigationModificationService;
    Arrangement = navigationArrangementService;
  }
}
