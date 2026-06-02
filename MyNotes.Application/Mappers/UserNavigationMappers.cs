using MyNotes.Application.Contracts.Database.Dtos.Navigations.Arrangement;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;
using MyNotes.Application.Contracts.Database.Queries.Navigations;
using MyNotes.Application.Dtos.Navigations.Arrangement;
using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Dtos.Navigations.Retrieval;
using MyNotes.Application.Queries.Navigations;
using MyNotes.Domain.Entities.Navigations;

namespace MyNotes.Application.Mappers;

internal static class UserNavigationMappers
{
  public static UserNavigationAppResponseDto ToAppDto(UserNavigationDbAggregateResponseDto userNavigationDbAggregateResponseDto)
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
}

