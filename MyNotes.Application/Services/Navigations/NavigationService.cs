namespace MyNotes.Application.Services.Navigations;

internal sealed partial class NavigationService
{
  public NavigationTreeService Tree { get; }
  public NavigationCreationService Creation { get; }
  public NavigationRetrievalService Retrieval { get; }
  public NavigationModificationService Modification { get; }
  public NavigationArrangementService Arrangement { get; }

  public NavigationService(NavigationTreeService navigationTreeService, NavigationCreationService navigationCreationService, NavigationRetrievalService navigationRetrievalService, NavigationModificationService navigationModificationService, NavigationArrangementService navigationArrangementService)
  {
    Tree = navigationTreeService;
    Creation = navigationCreationService;
    Retrieval = navigationRetrievalService;
    Modification = navigationModificationService;
    Arrangement = navigationArrangementService;
  }
}
