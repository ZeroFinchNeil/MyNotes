namespace MyNotes.Application.Dtos.Navigations;

internal record UserCompositeNavigationAppResponseDto : UserNavigationAppResponseDto
{
  public required IReadOnlyList<UserNavigationAppResponseDto> Children { get; init; }

  public required bool IsExpanded { get; init; }
}

/*
UserCompositeNavigationAppResponseDto dto = new()
{
  Id = ,
  Parent = ,
  Icon = ,
  Title = ,
  Position = ,
  IsDeleted = ,
  Children = ,
  IsExpanded = ,
};
*/