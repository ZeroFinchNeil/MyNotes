using System;

using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Infrastructure.Database.Entities.Navigations;

namespace MyNotes.Infrastructure.Mappers;

internal static class NavigationMappers
{
  public static UserNavigationEntity ToEntity(UserNavigationDbResponseDto userNavigationDbResponseDto) => throw new NotImplementedException();

  public static UserLeafNavigationViewStateEntity ToEntity(UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto) => throw new NotImplementedException();

  public static UserNavigationDbResponseDto ToDto(UserNavigationEntity userNavigationEntity) => throw new NotImplementedException();

  public static UserNavigationViewStateDbResponseDto ToDto(UserLeafNavigationViewStateEntity userLeafNavigationViewStateEntity) => throw new NotImplementedException();

}
