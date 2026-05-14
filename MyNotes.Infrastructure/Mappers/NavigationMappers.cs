using System;

using MyNotes.Application.Contracts.Database.Dtos.Navigations;
using MyNotes.Infrastructure.Database.Entities.Navigations;

namespace MyNotes.Infrastructure.Mappers;

internal static class NavigationMappers
{
  public static UserNavigationEntity ToEntity(UserNavigationDbResponseDto noteDbDto) => throw new NotImplementedException();
  public static UserLeafNavigationViewStateEntity ToEntity(UserNavigationViewStateDbResponseDto noteDbDto) => throw new NotImplementedException();
  public static UserNavigationDbResponseDto ToDto(UserNavigationEntity noteDbDto) => throw new NotImplementedException();
  public static UserNavigationViewStateDbResponseDto ToDto(UserLeafNavigationViewStateEntity noteDbDto) => throw new NotImplementedException();

}
