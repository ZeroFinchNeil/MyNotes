using MyNotes.Application.Contracts.Models.Navigations;
using MyNotes.Application.Services.Settings;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Services.Navigations;

internal sealed class NavigationFactory
{
  private readonly SettingsService SettingsService;

  public NavigationFactory(SettingsService settingsService) { SettingsService = settingsService; }

  public Navigation Create(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted) => new(id, parent, isComposite, icon, title, isDeleted);

  public Navigation Create(NavigationDto navigationDto) => Create(navigationDto.Id, navigationDto.ParentId, navigationDto.IsComposite, navigationDto.Icon, navigationDto.Title, navigationDto.IsDeleted);
}