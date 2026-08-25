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
using MyNotes.Application.Notes.Results;
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
using MyNotes.Services.Shell;
using MyNotes.Services.Updates;
using MyNotes.Services.Updates.Note;
using MyNotes.Services.Updates.NoteViewState;
using MyNotes.Services.Windows;
using MyNotes.Shell.Contracts.Windowing;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs.Providers;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes.Services;

internal static class ServiceCollectionExtension
{
  extension(IServiceCollection services)
  {
    public IServiceCollection ConfigureServices()
    {
      // Service
      services.AddAppCoreServices();
      services.AddWindowServices();
      services.AddSettingsService();
      services.AddNavigationServices();
      services.AddNoteServices();
      services.AddMediaServices();
      services.AddCommandServices();
      services.AddDbCoreServices();
      services.AddSearchCoreServices();

      // ViewModel
      services.AddViewModelProviders();
      services.AddViewModels();

      return services;
    }

    private void AddAppCoreServices()
    {
      services.AddSingleton<JumpListService>();
      services.AddSingleton<AppLogger>();
      services.AddSingleton<DialogService>();

      services.AddSingleton(TimeProvider.System);
      services.AddScoped(typeof(IUpdateCoordinator<,>), typeof(UpdateCoordinator<,>));
      services.AddScoped(typeof(IUpdateCoordinator<,,>), typeof(UpdateCoordinator<,,>));
      services.AddSingleton(typeof(IUpdateDispatcher<>), typeof(UpdateDispatcher<>));
      services.AddSingleton(typeof(IUpdateDispatcher<,>), typeof(UpdateDispatcher<,>));

      services.AddMemoryCache();
    }

    private void AddWindowServices()
    {
      services.AddSingleton<INativeWindowing, NativeWindowing>();
      services.AddSingleton<MainWindowService>();
      services.AddSingleton<NoteWindowService>();
      services.AddSingleton<ImageViewerWindowService>();
    }

    private void AddSettingsService()
    {
      services.AddSingleton<ISettingsStorage, SettingsStorage>();
      services.AddSingleton<AppSettingsService>();
    }

    private void AddNoteServices()
    {
      // Presentation
      services.AddSingleton<IModelFactory<NoteDto, NoteModel>, NoteModelFactory>();
      services.AddSingleton<IModelUpdater<NoteDto, NoteModel>, NoteModelUpdater>();
      services.AddSingleton<IModelStore<NoteId, NoteModel>, NoteModelStore>();

      services.AddScoped<IUpdateBatcher<string, NotePatchDto, UpdateNoteResult>, NoteUpdateBatcher>();
      services.AddSingleton<IUpdateHandler<NotePatchDto, UpdateNoteResult>, NoteUpdateHandler>();

      services.AddScoped<IUpdateBatcher<string, NoteViewStatePatchDto>, NoteViewStateUpdateBatcher>();
      services.AddSingleton<IUpdateHandler<NoteViewStatePatchDto>, NoteViewStateUpdateHandler>();

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

    private void AddNavigationServices()
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

    private void AddMediaServices()
    {
      services.AddSingleton<IImageRepository, ImageRepository>();
      services.AddSingleton<IImageFileStorage, ImageFileStorage>();
      services.AddSingleton<ImageService>();
    }

    private void AddCommandServices()
    {
      services.AddKeyedSingleton<ICommandService, NavigationCommandService>(CommandServiceType.Navigation);
      services.AddKeyedSingleton<ICommandService, NoteCommandService>(CommandServiceType.Note);
    }

    private void AddViewModels()
    {
      services.AddScoped<MainViewModel>();
      services.AddSingleton<SettingsViewModel>();
    }

    private void AddViewModelProviders()
    {
      services.AddSingleton<NavigationViewModelProvider>();

      services.AddSingleton<DialogViewModelProvider>();

      services.AddSingleton<NoteViewModelProvider>();
      services.AddSingleton<NoteEditorViewModelProvider>();
      services.AddSingleton<NotePreviewViewModelProvider>();
      services.AddSingleton<NotePreviewListViewModelProvider>();

      services.AddSingleton<ImageViewModelProvider>();
      services.AddSingleton<ImageCollectionViewModelProvider>();
    }

    private void AddDbCoreServices()
    {
      services.AddSingleton<AppDbContextTaskDispatcher>();
      services.AddDbContextFactory<AppDbContext>();
      services.AddScoped<AppDbContextInitializer>();
      services.AddSingleton<IAppDbTransactionFactory, AppDbTransactionFactory>();
    }

    private void AddSearchCoreServices()
    {
      services.AddSingleton<IRtfTextConverter, RtfTextConverter>();
      services.AddSingleton<AppSearchContext>();
    }
  }
}