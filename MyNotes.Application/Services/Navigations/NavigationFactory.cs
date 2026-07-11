using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Settings;

namespace MyNotes.Application.Services.Navigations;

internal sealed class NavigationFactory
{
  private readonly SettingsService SettingsService;
  
  public NavigationFactory(SettingsService settingsService) { SettingsService = settingsService; }

  public Navigation Create(NavigationId id, NavigationId parent, bool isComposite, int icon, string title, bool isDeleted) => new(id, parent, isComposite, icon, title, isDeleted);

  public Navigation Create(NavigationDbResponseDto dbResponseDto) => Create(dbResponseDto.Id, dbResponseDto.Parent, dbResponseDto.IsComposite, dbResponseDto.Icon, dbResponseDto.Title, dbResponseDto.IsDeleted);
}