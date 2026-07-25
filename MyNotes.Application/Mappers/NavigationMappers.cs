using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Application.Contracts.Enums.Navigations;
using MyNotes.Application.Contracts.Models.Navigations.Arrangement;
using MyNotes.Application.Contracts.Models.Navigations.Common;
using MyNotes.Application.Contracts.Models.Navigations.Creation;
using MyNotes.Application.Contracts.Models.Navigations.Modification;
using MyNotes.Application.Contracts.Models.Navigations.Queries;
using MyNotes.Application.Contracts.Models.Navigations.Retrieval;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Entities.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Application.Mappers;

[AssemblyLocal]
internal static class NavigationMappers
{
  public static NavigationAppResponseDto ToAppDto(NavigationDbResponseDto dbResponseDto) => dbResponseDto.IsComposite
    ? new CompositeNavigationAppResponseDto()
    {
      Id = dbResponseDto.Id,
      Parent = dbResponseDto.Parent,
      Icon = (Icon)dbResponseDto.Icon,
      Title = dbResponseDto.Title,
      IsDeleted = dbResponseDto.IsDeleted,
    }
    : new LeafNavigationAppResponseDto()
    {
      Id = dbResponseDto.Id,
      Parent = dbResponseDto.Parent,
      Icon = (Icon)dbResponseDto.Icon,
      Title = dbResponseDto.Title,
      IsDeleted = dbResponseDto.IsDeleted,
    };

  public static NavigationBundleAppResponseDto ToAppDto(NavigationBundleDbResponseDto bundleDbResponseDto)
  {
    var navigationDbDto = bundleDbResponseDto.NavigationDto;
    var viewStateDbDto = bundleDbResponseDto.ViewStateDto;
    return navigationDbDto.IsComposite
      ? new CompositeNavigationBundleAppResponseDto(
        navigationDto: (CompositeNavigationAppResponseDto)ToAppDto(navigationDbDto),
        viewStateDto: ToAppDto((CompositeNavigationViewStateDbResponseDto)viewStateDbDto),
        children: [])
      : new LeafNavigationBundleAppResponseDto(
        navigationDto: (LeafNavigationAppResponseDto)ToAppDto(navigationDbDto),
        viewStateDto: ToAppDto((LeafNavigationViewStateDbResponseDto)viewStateDbDto));
  }

  public static NavigationViewStateAppResponseDto ToAppDto(NavigationViewStateDbResponseDto dbResponseDto) => dbResponseDto switch
  {
    CompositeNavigationViewStateDbResponseDto compositeDto => ToAppDto(compositeDto),
    LeafNavigationViewStateDbResponseDto leafDto => ToAppDto(leafDto),
    _ => throw new InvalidOperationException()
  };

  public static CompositeNavigationViewStateAppResponseDto ToAppDto(CompositeNavigationViewStateDbResponseDto compositeDbResponseDto) => new()
  {
    Id = compositeDbResponseDto.Id,
    IsExpanded = compositeDbResponseDto.IsExpanded
  };

  public static LeafNavigationViewStateAppResponseDto ToAppDto(LeafNavigationViewStateDbResponseDto leafDbResponseDto) => new()
  {
    Id = leafDbResponseDto.Id,
    NoteSortKey = leafDbResponseDto.NoteSortKey,
    NoteSortDirection = leafDbResponseDto.NoteSortDirection,
    PreviewLayoutType = leafDbResponseDto.PreviewLayoutType,
    PreviewTileSize = leafDbResponseDto.PreviewTileSize,
    PreviewTileRatio = leafDbResponseDto.PreviewTileRatio
  };

  public static GetNavigationFieldValuesAppResponseDto ToAppDto(GetNavigationFieldValuesDbResponseDto getFieldsDbDto)
  {
    throw new NotImplementedException();
  }

  public static UpdateNavigationAppResponseDto ToAppDto(UpdateNavigationDbResponseDto updateDbResponseDto) => new()
  {
    ChangedFields = updateDbResponseDto.ChangedFields,
    Id = updateDbResponseDto.Id,
    Parent = updateDbResponseDto.Parent,
    Icon = updateDbResponseDto.Icon is int icon ? (Icon)icon : null,
    Title = updateDbResponseDto.Title is string title ? title : null,
    IsDeleted = updateDbResponseDto.IsDeleted is bool isDeleted ? isDeleted : null
  };

  public static FindNavigationsDbQuery ToDbQuery(FindNavigationsAppQuery findAppQuery)
  {
    throw new NotImplementedException();
  }

  public static CreateNavigationDbRequestDto ToCreateDbDto(Navigation navigation, NavigationId insertTargetId, NavigationInsertPosition insertPosition) => new()
  {
    Id = navigation.Id,
    ParentId = navigation.Parent,
    InsertTargetId = insertTargetId,
    InsertPosition = insertPosition,
    IsComposite = navigation.IsComposite,
    Icon = navigation.Icon,
    Title = navigation.Title
  };

  public static NavigationUpdateFields ToUpdateFields(NavigationChangedFields changedFields)
  {
    var updateFields = NavigationUpdateFields.None;
    if (changedFields == NavigationChangedFields.None)
    {
      return updateFields;
    }

    if (changedFields.HasFlag(NavigationChangedFields.Parent))
    {
      updateFields |= NavigationUpdateFields.Parent;
    }
    if (changedFields.HasFlag(NavigationChangedFields.Icon))
    {
      updateFields |= NavigationUpdateFields.Icon;
    }
    if (changedFields.HasFlag(NavigationChangedFields.Title))
    {
      updateFields |= NavigationUpdateFields.Title;
    }
    if (changedFields.HasFlag(NavigationChangedFields.IsDeleted))
    {
      updateFields |= NavigationUpdateFields.IsDeleted;
    }

    return updateFields;
  }

  public static UpdateNavigationDbRequestDto ToUpdateDbDto(Navigation navigation, NavigationUpdateFields updateFields)
  {
    UpdateNavigationDbRequestDto dto = new()
    {
      UpdateFields = updateFields,
      Id = navigation.Id,
      IsComposite = navigation.IsComposite
    };

    if (updateFields is NavigationUpdateFields.None)
    {
      return dto;
    }

    if (updateFields.HasFlag(NavigationUpdateFields.Parent))
    {
      dto.Parent = navigation.Parent;
    }
    if (updateFields.HasFlag(NavigationUpdateFields.Icon))
    {
      dto.Icon = navigation.Icon;
    }
    if (updateFields.HasFlag(NavigationUpdateFields.Title))
    {
      dto.Title = navigation.Title;
    }
    if (updateFields.HasFlag(NavigationUpdateFields.IsDeleted))
    {
      dto.IsDeleted = navigation.IsDeleted;
    }

    return dto;
  }

  public static DeleteNavigationDbRequestDto ToDbDto(DeleteNavigationAppRequestDto deleteAppRequestDto) => new()
  {
    Id = deleteAppRequestDto.Id,
    DeleteMode = deleteAppRequestDto.DeleteMode
  };

  public static MoveNavigationDbRequestDto ToDbDto(MoveNavigationAppRequestDto moveAppRequestDto) => new()
  {
    SourceNavigation = moveAppRequestDto.SourceNavigation,
    TargetNavigation = moveAppRequestDto.TargetNavigation,
    InsertPosition = moveAppRequestDto.InsertPosition
  };

  public static NavigationBundleDbResponseDto BundleDbDto(NavigationDbResponseDto dbResponseDto, NavigationViewStateDbResponseDto viewStateDbResponseDto)
  {
    // composite와 leaf 구성에 따라 달리 구현
    throw new NotImplementedException();
  }

  public static NavigationBundleAppResponseDto BundleAppDto(NavigationAppResponseDto navigationAppResponseDto, NavigationViewStateAppResponseDto viewStateAppResponseDto) => (navigationAppResponseDto, viewStateAppResponseDto) switch
  {
    (CompositeNavigationAppResponseDto compositeNavigation, CompositeNavigationViewStateAppResponseDto compositeViewState) => new CompositeNavigationBundleAppResponseDto(compositeNavigation, compositeViewState, []),
    (LeafNavigationAppResponseDto leafNavigation, LeafNavigationViewStateAppResponseDto leafViewState) => new LeafNavigationBundleAppResponseDto(leafNavigation, leafViewState),
    _ => throw new InvalidOperationException(),
  };

  public static UpdateNavigationViewStateDbRequestDto ToDbDto(UpdateNavigationViewStateAppRequestDto updateAppRequestDto) => updateAppRequestDto switch
  {
    UpdateCompositeNavigationViewStateAppRequestDto compositeDto => new UpdateCompositeNavigationViewStateDbRequestDto()
    {
      Id = compositeDto.Id,
      UpdateFields = compositeDto.UpdateFields,
      IsExpanded = compositeDto.IsExpanded
    },
    UpdateLeafNavigationViewStateAppRequestDto leafDto => new UpdateLeafNavigationViewStateDbRequestDto()
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

