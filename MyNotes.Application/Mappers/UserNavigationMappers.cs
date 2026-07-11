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
  public static UserNavigationAppResponseDto ToAppDto(UserNavigationDbResponseDto dbResponseDto) => dbResponseDto.IsComposite
    ? new UserCompositeNavigationAppResponseDto()
    {
      Id = dbResponseDto.Id,
      Parent = dbResponseDto.Parent,
      Icon = (Icon)dbResponseDto.Icon,
      Title = dbResponseDto.Title,
      IsDeleted = dbResponseDto.IsDeleted,
    }
    : new UserLeafNavigationAppResponseDto()
    {
      Id = dbResponseDto.Id,
      Parent = dbResponseDto.Parent,
      Icon = (Icon)dbResponseDto.Icon,
      Title = dbResponseDto.Title,
      IsDeleted = dbResponseDto.IsDeleted,
    };

  public static UserNavigationBundleAppResponseDto ToAppDto(UserNavigationBundleDbResponseDto bundleDbResponseDto)
  {
    var userNavigationDbDto = bundleDbResponseDto.UserNavigationDto;
    var viewStateDbDto = bundleDbResponseDto.ViewStateDto;
    return userNavigationDbDto.IsComposite
      ? new UserCompositeNavigationBundleAppResponseDto(
        userNavigationDto: (UserCompositeNavigationAppResponseDto)ToAppDto(userNavigationDbDto),
        viewStateDto: ToAppDto((UserCompositeNavigationViewStateDbResponseDto)viewStateDbDto),
        children: [])
      : new UserLeafNavigationBundleAppResponseDto(
        userNavigationDto: (UserLeafNavigationAppResponseDto)ToAppDto(userNavigationDbDto),
        viewStateDto: ToAppDto((UserLeafNavigationViewStateDbResponseDto)viewStateDbDto));
  }

  public static UserNavigationViewStateAppResponseDto ToAppDto(UserNavigationViewStateDbResponseDto dbResponseDto) => dbResponseDto switch
  {
    UserCompositeNavigationViewStateDbResponseDto compositeDto => ToAppDto(compositeDto),
    UserLeafNavigationViewStateDbResponseDto leafDto => ToAppDto(leafDto),
    _ => throw new InvalidOperationException()
  };

  public static UserCompositeNavigationViewStateAppResponseDto ToAppDto(UserCompositeNavigationViewStateDbResponseDto compositeDbResponseDto) => new()
  {
    Id = compositeDbResponseDto.Id,
    IsExpanded = compositeDbResponseDto.IsExpanded
  };

  public static UserLeafNavigationViewStateAppResponseDto ToAppDto(UserLeafNavigationViewStateDbResponseDto leafDbResponseDto) => new()
  {
    Id = leafDbResponseDto.Id,
    NoteSortKey = leafDbResponseDto.NoteSortKey,
    NoteSortDirection = leafDbResponseDto.NoteSortDirection,
    PreviewLayoutType = leafDbResponseDto.PreviewLayoutType,
    PreviewTileSize = leafDbResponseDto.PreviewTileSize,
    PreviewTileRatio = leafDbResponseDto.PreviewTileRatio
  };

  public static GetUserNavigationFieldValuesAppResponseDto ToAppDto(GetUserNavigationFieldValuesDbResponseDto getFieldsDbDto)
  {
    throw new NotImplementedException();
  }

  public static UpdateUserNavigationAppResponseDto ToAppDto(UpdateUserNavigationDbResponseDto updateDbResponseDto) => new()
  {
    ChangedNavigationFields = updateDbResponseDto.ChangedNavigationFields,
    Id = updateDbResponseDto.Id,
    Parent = updateDbResponseDto.Parent,
    Icon = updateDbResponseDto.Icon is int icon ? (Icon)icon : null,
    Title = updateDbResponseDto.Title is string title ? title : null,
    IsDeleted = updateDbResponseDto.IsDeleted is bool isDeleted ? isDeleted : null
  };

  public static FindUserNavigationsDbQuery ToDbQuery(FindUserNavigationsAppQuery findAppQuery)
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

  public static DeleteUserNavigationDbRequestDto ToDbDto(DeleteUserNavigationAppRequestDto deleteAppRequestDto) => new()
  {
    Id = deleteAppRequestDto.Id,
    DeleteMode = deleteAppRequestDto.DeleteMode
  };

  public static MoveUserNavigationDbRequestDto ToDbDto(MoveUserNavigationAppRequestDto moveAppRequestDto) => new()
  {
    SourceNavigation = moveAppRequestDto.SourceNavigation,
    TargetNavigation = moveAppRequestDto.TargetNavigation,
    NavigationInsertPosition = moveAppRequestDto.NavigationInsertPosition
  };

  public static UserNavigationBundleDbResponseDto BundleDbDto(UserNavigationDbResponseDto userNavigationDbResponseDto, UserNavigationViewStateDbResponseDto viewStateDbResponseDto)
  {
    // composite와 leaf 구성에 따라 달리 구현
    throw new NotImplementedException();
  }

  public static UserNavigationBundleAppResponseDto BundleAppDto(UserNavigationAppResponseDto userNavigationAppResponseDto, UserNavigationViewStateAppResponseDto viewStateAppResponseDto) => (userNavigationAppResponseDto, viewStateAppResponseDto) switch
  {
    (UserCompositeNavigationAppResponseDto compositeNavigation, UserCompositeNavigationViewStateAppResponseDto compositeViewState) => new UserCompositeNavigationBundleAppResponseDto(compositeNavigation, compositeViewState, []),
    (UserLeafNavigationAppResponseDto leafNavigation, UserLeafNavigationViewStateAppResponseDto leafViewState) => new UserLeafNavigationBundleAppResponseDto(leafNavigation, leafViewState),
    _ => throw new InvalidOperationException(),
  };

  public static UpdateUserNavigationViewStateDbRequestDto ToDbDto(UpdateUserNavigationViewStateAppRequestDto updateAppRequestDto) => updateAppRequestDto switch
  {
    UpdateUserCompositeNavigationViewStateAppRequestDto compositeDto => new UpdateUserCompositeNavigationViewStateDbRequestDto()
    {
      Id = compositeDto.Id,
      UpdateFields = compositeDto.UpdateFields,
      IsExpanded = compositeDto.IsExpanded
    },
    UpdateUserLeafNavigationViewStateAppRequestDto leafDto => new UpdateUserLeafNavigationViewStateDbRequestDto()
    {
      Id = leafDto.Id,
      UpdateFields = leafDto.UpdateFields,
      NoteSortKey = leafDto.NoteSortKey,
      NoteSortDirection = leafDto.NoteSortDirection,
      PreviewLayoutType = leafDto.PreviewLayoutType,
      PreviewTileSize = leafDto.PreviewTileSize,
      PreviewTileRatio = leafDto.PreviewTileRatio
    },
    _ => throw new InvalidOperationException()
  };
}

