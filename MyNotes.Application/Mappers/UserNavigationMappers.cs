using MyNotes.Application.Contracts.Database.Dtos.Navigations.Arrangement;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Queries;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;
using MyNotes.Application.Dtos.Navigations.Arrangement;
using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Dtos.Navigations.Queries;
using MyNotes.Application.Dtos.Navigations.Retrieval;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Entities.Navigations;

namespace MyNotes.Application.Mappers;

[AssemblyLocal]
internal static class UserNavigationMappers
{
  public static UserNavigationAppResponseDto ToAppDto(UserNavigationDbResponseDto userNavigationDbResponseDto)
  {
    throw new NotImplementedException();
  }

  public static UserNavigationBundleAppResponseDto ToAppDto(UserNavigationBundleDbResponseDto userNavigationBundleDbResponseDto)
  {
    throw new NotImplementedException();
  }

  public static UserNavigationViewStateAppResponseDto ToAppDto(UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto)
  {
    throw new NotImplementedException();
  }

  public static GetUserNavigationFieldValuesAppResponseDto ToAppDto(GetUserNavigationFieldValuesDbResponseDto getUserNavigationFieldsDbDto)
  {
    throw new NotImplementedException();
  }

  public static UserNavigation ToDomainEntity(UpdateUserNavigationAppRequestDto updateUserNavigationAppRequestDto)
  {
    throw new NotImplementedException();
  }

  public static UpdateUserNavigationAppResponseDto ToAppDto(UpdateUserNavigationDbResponseDto updateUserNavigationDbResponseDto)
  {
    throw new NotImplementedException();
  }

  public static FindUserNavigationsDbQuery ToDbQuery(FindUserNavigationsAppQuery findUserNavigationsAppQuery)
  {
    throw new NotImplementedException();
  }

  public static UpdateUserNavigationDbRequestDto ToDbDto(UserNavigation userNavigation)
  {
    throw new NotImplementedException();
  }

  public static DeleteUserNavigationDbRequestDto ToDbDto(DeleteUserNavigationAppRequestDto deleteUserNavigationAppRequestDto)
  {
    throw new NotImplementedException();
  }

  public static MoveUserNavigationDbRequestDto ToDbDto(MoveUserNavigationAppRequestDto moveUserNavigationAppRequestDto)
  {
    throw new NotImplementedException();
  }

  public static UserNavigationBundleDbResponseDto BundleDbDto(UserNavigationDbResponseDto userNavigationDbResponseDto, UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto)
  {
    // composite와 leaf 구성에 따라 달리 구현
    throw new NotImplementedException();
  }

  public static UserNavigationBundleAppResponseDto BundleAppDto(UserNavigationAppResponseDto userNavigationAppResponseDto, UserNavigationViewStateAppResponseDto userNavigationViewStateAppResponseDto)
  {
    // composite와 leaf 구성에 따라 달리 구현
    throw new NotImplementedException();
  }
}

