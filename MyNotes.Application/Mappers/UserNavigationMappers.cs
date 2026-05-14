using MyNotes.Application.Contracts.Database.Dtos.Navigations;
using MyNotes.Application.Contracts.Database.Queries.Navigations;
using MyNotes.Application.Dtos.Navigations;
using MyNotes.Application.Queries.Navigations;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Templates;

namespace MyNotes.Application.Mappers;

internal static class UserNavigationMappers
{
  public static UserNavigationAppResponseDto ToAppDto(UserNavigationDbResponseDto userNavigationDbResponseDto) => userNavigationDbResponseDto.IsComposite
    ? new UserCompositeNavigationAppResponseDto()
    {
      Id = userNavigationDbResponseDto.Id,
      Parent = userNavigationDbResponseDto.Parent,
      Icon = (Icon)userNavigationDbResponseDto.Icon,
      Title = userNavigationDbResponseDto.Title,
      Position = userNavigationDbResponseDto.Position,
      Children = [],
    }
    : new UserLeafNavigationAppResponseDto()
    {
      Id = userNavigationDbResponseDto.Id,
      Parent = userNavigationDbResponseDto.Parent,
      Icon = (Icon)userNavigationDbResponseDto.Icon,
      Title = userNavigationDbResponseDto.Title,
      Position = userNavigationDbResponseDto.Position
    };

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

