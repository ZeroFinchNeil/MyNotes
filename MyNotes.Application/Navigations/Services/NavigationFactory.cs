using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Settings.Services;
using MyNotes.Domain.Navigations;

namespace MyNotes.Application.Navigations.Services;

internal sealed class NavigationFactory
{
  private readonly AppSettingsService SettingsService;

  public NavigationFactory(AppSettingsService settingsService) { SettingsService = settingsService; }

  public Navigation Create(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted) => new(id, parent, isComposite, icon, title, isDeleted);

  public Navigation Create(NavigationDto navigationDto) => Create(navigationDto.Id, navigationDto.ParentId, navigationDto.IsComposite, navigationDto.Icon, navigationDto.Title, navigationDto.IsDeleted);
}