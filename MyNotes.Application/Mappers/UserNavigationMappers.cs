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
using MyNotes.Common.Querying;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Templates;

namespace MyNotes.Application.Mappers;

[AssemblyLocal]
internal static class UserNavigationMappers
{
  public static UserNavigationAppResponseDto ToAppDto(UserNavigationDbResponseDto userNavigationDbResponseDto) => userNavigationDbResponseDto.IsComposite
    ? new UserCompositeNavigationAppResponseDto()
    {
      Id = userNavigationDbResponseDto.Id,
      Parent = userNavigationDbResponseDto.Parent,
      Icon = (Icon)userNavigationDbResponseDto.Icon,
      Title = userNavigationDbResponseDto.Title,
      IsDeleted = userNavigationDbResponseDto.IsDeleted,
    }
    : new UserLeafNavigationAppResponseDto()
    {
      Id = userNavigationDbResponseDto.Id,
      Parent = userNavigationDbResponseDto.Parent,
      Icon = (Icon)userNavigationDbResponseDto.Icon,
      Title = userNavigationDbResponseDto.Title,
      IsDeleted = userNavigationDbResponseDto.IsDeleted,
    };

  public static UserNavigationBundleAppResponseDto ToAppDto(UserNavigationBundleDbResponseDto userNavigationBundleDbResponseDto)
  {
    var userNavigationDbDto = userNavigationBundleDbResponseDto.UserNavigationDto;
    var viewStateDbDto = userNavigationBundleDbResponseDto.ViewStateDto;
    return userNavigationDbDto.IsComposite
      ? new UserCompositeNavigationBundleAppResponseDto(
        userNavigationDto: (UserCompositeNavigationAppResponseDto)ToAppDto(userNavigationDbDto),
        viewStateDto: ToAppDto((UserCompositeNavigationViewStateDbResponseDto)viewStateDbDto),
        children: [])
      : new UserLeafNavigationBundleAppResponseDto(
        userNavigationDto: (UserLeafNavigationAppResponseDto)ToAppDto(userNavigationDbDto),
        viewStateDto: ToAppDto((UserLeafNavigationViewStateDbResponseDto)viewStateDbDto));
  }

  public static UserNavigationViewStateAppResponseDto ToAppDto(UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto) => userNavigationViewStateDbResponseDto switch
  {
    UserCompositeNavigationViewStateDbResponseDto compositeDto => ToAppDto(compositeDto),
    UserLeafNavigationViewStateDbResponseDto leafDto => ToAppDto(leafDto),
    _ => throw new InvalidOperationException()
  };

  public static UserCompositeNavigationViewStateAppResponseDto ToAppDto(UserCompositeNavigationViewStateDbResponseDto userCompositeNavigationViewStateDbResponseDto) => new()
  {
    Id = userCompositeNavigationViewStateDbResponseDto.Id,
    IsExpanded = userCompositeNavigationViewStateDbResponseDto.IsExpanded
  };

  public static UserLeafNavigationViewStateAppResponseDto ToAppDto(UserLeafNavigationViewStateDbResponseDto userCompositeNavigationViewStateDbResponseDto) => new()
  {
    Id = userCompositeNavigationViewStateDbResponseDto.Id,
    NoteSortKey = (NoteSortKey?)userCompositeNavigationViewStateDbResponseDto.NoteSortKey,
    NoteSortDirection = (SortDirection?)userCompositeNavigationViewStateDbResponseDto.NoteSortDirection,
    PreviewLayoutType = (PreviewLayoutType?)userCompositeNavigationViewStateDbResponseDto.PreviewLayoutType,
    PreviewTileSize = (PreviewTileSize?)userCompositeNavigationViewStateDbResponseDto.PreviewTileSize,
    PreviewTileRatio = (PreviewTileRatio?)userCompositeNavigationViewStateDbResponseDto.PreviewTileRatio
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

  public static MoveUserNavigationDbRequestDto ToDbDto(MoveUserNavigationAppRequestDto moveUserNavigationAppRequestDto) => new()
  {
    SourceNavigation = moveUserNavigationAppRequestDto.SourceNavigation,
    TargetNavigation = moveUserNavigationAppRequestDto.TargetNavigation,
    NavigationInsertPosition = moveUserNavigationAppRequestDto.NavigationInsertPosition
  };

  public static UserNavigationBundleDbResponseDto BundleDbDto(UserNavigationDbResponseDto userNavigationDbResponseDto, UserNavigationViewStateDbResponseDto userNavigationViewStateDbResponseDto)
  {
    // composite와 leaf 구성에 따라 달리 구현
    throw new NotImplementedException();
  }

  public static UserNavigationBundleAppResponseDto BundleAppDto(UserNavigationAppResponseDto userNavigationAppResponseDto, UserNavigationViewStateAppResponseDto userNavigationViewStateAppResponseDto) => (userNavigationAppResponseDto, userNavigationViewStateAppResponseDto) switch
  {
    (UserCompositeNavigationAppResponseDto compositeNavigation, UserCompositeNavigationViewStateAppResponseDto compositeViewState) => new UserCompositeNavigationBundleAppResponseDto(compositeNavigation, compositeViewState, []),
    (UserLeafNavigationAppResponseDto leafNavigation, UserLeafNavigationViewStateAppResponseDto leafViewState) => new UserLeafNavigationBundleAppResponseDto(leafNavigation, leafViewState),
    _ => throw new InvalidOperationException(),
  };
}

