using MyNotes.Domain.ValueObjects;

namespace MyNotes.Application.Dtos.Navigations.Common;

//todo: C#15의 closed 키워드 이용하여 switch 분기 명확히 하기
internal abstract record UserNavigationViewStateAppResponseDto
{
  public required NavigationId Id { get; init; }
}