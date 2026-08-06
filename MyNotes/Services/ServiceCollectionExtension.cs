using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Converters;
using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Application.Contracts.Navigations.Persistence;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Application.Contracts.Settings;
using MyNotes.Application.Media.Services;
using MyNotes.Application.Navigations.Services;
using MyNotes.Application.Notes.Services;
using MyNotes.Application.Settings.Services;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Converters;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Repositories.Media;
using MyNotes.Infrastructure.Database.Repositories.Navigations;
using MyNotes.Infrastructure.Database.Repositories.Notes;
using MyNotes.Infrastructure.Logging;
using MyNotes.Infrastructure.Search.Core;
using MyNotes.Infrastructure.Search.Repositories.Notes;
using MyNotes.Infrastructure.Storage.Media;
using MyNotes.Infrastructure.Storage.Settings;
using MyNotes.Infrastructure.Windowing;
using MyNotes.Models;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Settings;
using MyNotes.Services.Shell;
using MyNotes.Services.ViewState;
using MyNotes.Services.ViewState.Batching;
using MyNotes.Services.ViewState.Dispatcher;
using MyNotes.Services.Windows;
using MyNotes.Shell.Contracts.Windowing;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Services;

internal static class ServiceCollectionExtension
{
  extension(IServiceCollection services)
  {
    public void AddAppCoreServices()
    {
      services.AddSingleton<JumpListService>();
      services.AddSingleton<AppLogger>();
      services.AddSingleton<DialogService>();

      services.AddSingleton(TimeProvider.System);
      services.AddScoped(typeof(IViewStatePersistenceCoordinator<>), typeof(ViewStatePersistenceCoordinator<>));
    }

    public void AddWindowServices()
    {
      services.AddSingleton<INativeWindowing, NativeWindowing>();
      services.AddSingleton<MainWindowService>();
      services.AddSingleton<NoteWindowService>();
      services.AddSingleton<ImageViewerWindowService>();
    }

    public void AddSettingsService()
    {
      services.AddSingleton<ISettingsStorage, SettingsStorage>();
      services.AddSingleton<AppSettingsService>();
      services.AddSingleton<ViewStateSettingsService>();
    }

    public void AddNoteServices()
    {
      // Presentation
      services.AddSingleton<IModelFactory<NoteDto, NoteModel>, NoteModelFactory>();
      services.AddSingleton<IModelUpdater<NoteDto, NoteModel>, NoteModelUpdater>();
      services.AddSingleton<IModelStore<NoteId, NoteModel>, NoteModelStore>();

      services.AddScoped<IViewStatePatchBatcher<string, NoteViewStatePatchDto>, NoteViewStatePatchBatcher>();
      services.AddScoped<IViewStatePersistenceDispatcher<NoteViewStatePatchDto>, NoteViewStatePersistenceDispatcher>();

      // Application
      services.AddSingleton<NoteService>();

      services.AddSingleton<NoteCreationService>();
      services.AddSingleton<NoteRetrievalService>();
      services.AddSingleton<NoteModificationService>();

      services.AddSingleton<NoteFactory>();

      // Infrastructure
      services.AddSingleton<INoteRepository, NoteRepository>();
      services.AddSingleton<INoteSearcher, NoteSearcher>();
    }

    public void AddNavigationServices()
    {
      services.AddSingleton<INavigationRepository, NavigationRepository>();

      services.AddSingleton<NavigationService>();

      services.AddSingleton<NavigationArrangementService>();
      services.AddSingleton<NavigationCreationService>();
      services.AddSingleton<NavigationRetrievalService>();
      services.AddSingleton<NavigationModificationService>();

      services.AddSingleton<NavigationFactory>();

      services.AddSingleton<NavigationController>();
    }

    public void AddMediaServices()
    {
      services.AddSingleton<IImageRepository, ImageRepository>();
      services.AddSingleton<IImageFileStorage, ImageFileStorage>();
      services.AddSingleton<ImageService>();
    }

    public void AddCommandServices()
    {
      services.AddKeyedSingleton<ICommandService, NavigationCommandService>(CommandServiceType.Navigation);
      services.AddKeyedSingleton<ICommandService, NoteCommandService>(CommandServiceType.Note);
    }

    public void AddViewModels()
    {
      services.AddScoped<MainViewModel>();
      services.AddSingleton<SettingsViewModel>();
    }

    public void AddViewModelProviders()
    {
      services.AddSingleton<NavigationViewModelProvider>();

      services.AddSingleton<DialogViewModelFactory>();

      services.AddSingleton<NoteViewModelProvider>();
      services.AddSingleton<NoteEditorViewModelProvider>();
      services.AddSingleton<NoteListViewModelProvider>();

      services.AddSingleton<ImageViewModelProvider>();
      services.AddSingleton<ImageCollectionViewModelProvider>();
    }

    public void AddDbCoreServices()
    {
      services.AddSingleton<AppDbContextTaskDispatcher>();
      services.AddDbContextFactory<AppDbContext>();
      services.AddScoped<AppDbContextInitializer>();
      services.AddSingleton<IAppDbTransactionFactory, AppDbTransactionFactory>();
    }

    public void AddSearchCoreServices()
    {
      services.AddSingleton<IRtfTextConverter, RtfTextConverter>();
      services.AddSingleton<AppSearchContext>();
    }
  }
}