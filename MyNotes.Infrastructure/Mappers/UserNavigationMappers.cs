using System;

using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Debugging.Attributes;
using MyNotes.Infrastructure.Database.Entities.Navigations;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class UserNavigationMappers
{
  public static UserNavigationEntity ToEntity(UserNavigationDbResponseDto userNavigationDbResponseDto) => throw new NotImplementedException();

  public static UserCompositeNavigationViewStateEntity ToEntity(UserCompositeNavigationViewStateDbResponseDto userCompositeNavigationViewStateDbResponseDto) => throw new NotImplementedException();

  public static UserLeafNavigationViewStateEntity ToEntity(UserLeafNavigationViewStateDbResponseDto userLeafNavigationViewStateDbResponseDto) => throw new NotImplementedException();

  public static UserCompositeNavigationViewStateDbResponseDto ToDto(UserCompositeNavigationViewStateEntity userCompositeNavigationViewStateEntity) => throw new NotImplementedException();

  public static UserLeafNavigationViewStateDbResponseDto ToDto(UserLeafNavigationViewStateEntity userLeafNavigationViewStateEntity) => throw new NotImplementedException();

  public static UserNavigationDbResponseDto ToDto(UserNavigationEntity userNavigationEntity) => throw new NotImplementedException();
}
