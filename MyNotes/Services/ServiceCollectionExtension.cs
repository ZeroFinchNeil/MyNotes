using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Application.Contracts.Persistence.Navigations;
using MyNotes.Application.Contracts.Persistence.Notes;
using MyNotes.Application.Services.Navigations;
using MyNotes.Application.Services.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Converters;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Repositories.Navigations;
using MyNotes.Infrastructure.Database.Repositories.Notes;
using MyNotes.Infrastructure.Search.Core;
using MyNotes.Infrastructure.Search.Repositories.Notes;
using MyNotes.Infrastructure.Windowing;
using MyNotes.Models;
using MyNotes.Models.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
using MyNotes.Services.Windows;
using MyNotes.Shell.Contracts.Converters;
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
    public void AddWindowServices()
    {
      services.AddSingleton<INativeWindowing, NativeWindowing>();
      services.AddSingleton<MainWindowService>();
      services.AddSingleton<NoteWindowService>();
      services.AddSingleton<ImageViewerWindowService>();
    }

    public void AddNoteServices()
    {
      // Presentation
      services.AddSingleton<IModelFactory<NoteDto, NoteModel>, NoteModelFactory>();
      services.AddSingleton<IModelUpdater<NoteDto, NoteModel>, NoteModelUpdater>();
      services.AddSingleton<IModelStore<NoteId, NoteModel>, NoteModelStore>();

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

    public void AddCommandServices()
    {
      services.AddKeyedSingleton<ICommandService, NavigationCommandService>(CommandServiceType.Navigation);
      services.AddKeyedSingleton<ICommandService, NoteCommandService>(CommandServiceType.Note);
    }

    public void AddViewModels()
    {
      services.AddScoped<MainViewModel>();
      services.AddScoped<SettingsViewModel>();
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