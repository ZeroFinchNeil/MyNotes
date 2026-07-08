using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Settings;

namespace MyNotes.Application.Services.Navigations;

internal sealed class UserNavigationFactory
{
  private readonly SettingsService SettingsService;
  
  public UserNavigationFactory(SettingsService settingsService) { SettingsService = settingsService; }

  public UserNavigation Create(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted) => new(id, parent, isComposite, icon, title, isDeleted);

  public UserNavigation Create(UserNavigationDbResponseDto dto) => Create(dto.Id, dto.Parent, dto.IsComposite, dto.Icon, dto.Title, dto.IsDeleted);
}