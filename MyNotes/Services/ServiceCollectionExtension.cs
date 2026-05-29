using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Database.Core;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Notes;
using MyNotes.Application.Contracts.Search.Repositories.Notes;
using MyNotes.Application.Services.Navigations;
using MyNotes.Application.Services.Notes;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Repositories.Navigations;
using MyNotes.Infrastructure.Database.Repositories.Notes;
using MyNotes.Infrastructure.Search.Core;
using MyNotes.Infrastructure.Search.Repositories.Notes;
using MyNotes.Services.Commands;
using MyNotes.Services.Navigations;
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
    public void AddNoteServices()
    {
      services.AddSingleton<INoteRepository, NoteRepository>();
      services.AddSingleton<INoteSearcher, NoteSearcher>();

      services.AddSingleton<NoteService>();

      services.AddSingleton<NoteCreationService>();
      services.AddSingleton<NoteRetrievalService>();
      services.AddSingleton<NoteModificationService>();

      services.AddSingleton<NoteFactory>();
    }

    public void AddNavigationServices()
    {
      services.AddSingleton<INavigationRepository, NavigationRepository>();

      services.AddSingleton<NavigationService>();

      services.AddSingleton<NavigationTreeService>();
      services.AddSingleton<NavigationCreationService>();
      services.AddSingleton<NavigationRetrievalService>();
      services.AddSingleton<NavigationModificationService>();

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
      services.AddTransient<IAppDbTransaction, AppDbTransaction>();
    }

    public void AddSearchCoreServices()
    {
      services.AddSingleton<AppSearchContext>();
    }
  }
}