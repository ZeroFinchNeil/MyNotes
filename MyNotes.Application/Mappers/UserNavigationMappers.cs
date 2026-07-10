using MyNotes.Application.Contracts.Database.Dtos.Navigations.Arrangement;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Common;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Queries;
using MyNotes.Application.Contracts.Database.Dtos.Navigations.Retrieval;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Dtos.Navigations.Arrangement;
using MyNotes.Application.Dtos.Navigations.Common;
using MyNotes.Application.Dtos.Navigations.Modification;
using MyNotes.Application.Dtos.Navigations.Queries;
using MyNotes.Application.Dtos.Navigations.Retrieval;
using MyNotes.Common.Querying;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;
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

  public static UpdateUserNavigationAppResponseDto ToAppDto(UpdateUserNavigationDbResponseDto updateUserNavigationDbResponseDto) => new()
  {
    ChangedNavigationFields = updateUserNavigationDbResponseDto.ChangedNavigationFields,
    Id = updateUserNavigationDbResponseDto.Id,
    Parent = updateUserNavigationDbResponseDto.Parent,
    Icon = updateUserNavigationDbResponseDto.Icon is int icon ? (Icon)icon : null,
    Title = updateUserNavigationDbResponseDto.Title is string title ? title : null,
    IsDeleted = updateUserNavigationDbResponseDto.IsDeleted is bool isDeleted ? isDeleted : null
  };

  public static FindUserNavigationsDbQuery ToDbQuery(FindUserNavigationsAppQuery findUserNavigationsAppQuery)
  {
    throw new NotImplementedException();
  }

  public static CreateUserNavigationDbRequestDto ToCreateDbDto(UserNavigation userNavigation, NavigationId insertTargetId, NavigationInsertPosition insertPosition) => new()
  {
    Id = userNavigation.Id,
    ParentId = userNavigation.Parent,
    InsertTargetId = insertTargetId,
    NavigationInsertPosition = insertPosition,
    IsComposite = userNavigation.IsComposite,
    Icon = userNavigation.Icon,
    Title = userNavigation.Title
  };

  public static UserNavigationUpdateFields ToUpdateFields(UserNavigationChangedFields changedFields)
  {
    var updateFields = UserNavigationUpdateFields.None;
    if (changedFields == UserNavigationChangedFields.None)
    {
      return updateFields;
    }

    if (changedFields.HasFlag(UserNavigationChangedFields.Parent))
    {
      updateFields |= UserNavigationUpdateFields.Parent;
    }
    if (changedFields.HasFlag(UserNavigationChangedFields.Icon))
    {
      updateFields |= UserNavigationUpdateFields.Icon;
    }
    if (changedFields.HasFlag(UserNavigationChangedFields.Title))
    {
      updateFields |= UserNavigationUpdateFields.Title;
    }
    if (changedFields.HasFlag(UserNavigationChangedFields.IsDeleted))
    {
      updateFields |= UserNavigationUpdateFields.IsDeleted;
    }

    return updateFields;
  }

  public static UpdateUserNavigationDbRequestDto ToUpdateDbDto(UserNavigation userNavigation, UserNavigationUpdateFields updateFields)
  {
    UpdateUserNavigationDbRequestDto dto = new()
    {
      NavigationUpdateFields = updateFields,
      Id = userNavigation.Id,
      IsComposite = userNavigation.IsComposite
    };

    if (updateFields is UserNavigationUpdateFields.None)
    {
      return dto;
    }

    if (updateFields.HasFlag(UserNavigationUpdateFields.Parent))
    {
      dto.Parent = userNavigation.Parent;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.Icon))
    {
      dto.Icon = userNavigation.Icon;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.Title))
    {
      dto.Title = userNavigation.Title;
    }
    if (updateFields.HasFlag(UserNavigationUpdateFields.IsDeleted))
    {
      dto.IsDeleted = userNavigation.IsDeleted;
    }

    return dto;
  }

  public static DeleteUserNavigationDbRequestDto ToDbDto(DeleteUserNavigationAppRequestDto deleteUserNavigationAppRequestDto) => new()
  {
    Id = deleteUserNavigationAppRequestDto.Id,
    DeleteMode = deleteUserNavigationAppRequestDto.DeleteMode
  };

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

