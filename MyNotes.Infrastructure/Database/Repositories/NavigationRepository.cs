using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Dtos.Navigations;
using MyNotes.Application.Contracts.Database.Enums.Navigations;
using MyNotes.Application.Contracts.Database.Repositories;
using MyNotes.Application.Contracts.Database.Repositories.Navigations;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Database.Entities.Navigations;

namespace MyNotes.Infrastructure.Database.Repositories;

internal class NavigationRepository : INavigationRepository
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public NavigationRepository(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public Task<IReadOnlyList<UserNavigationDbResponseDto>> GetUserNavigationsAsync()
  {
    throw new NotImplementedException();
  }

  public Task<NavigationId> GenerateUniqueUserNavigationIdAsync()
  {
    throw new NotImplementedException();
  }

  public Task<GetUserNavigationFieldValuesDbResponseDto> GetUserNavigationFieldsAsync(GetUserNavigationFieldValuesDbRequestDto getUserNavigationFieldsDbRequestDto)
  {
    throw new NotImplementedException();
  }

  public Task<UserNavigationDbAggregateResponseDto> AddUserNavigationAsync(CreateUserNavigationDbRequestDto createUserNavigationDbRequestDto)
  {
    throw new NotImplementedException();
  }

  public Task<UpdateUserNavigationDbResponseDto> UpdateUserNavigationAsync(UpdateUserNavigationDbRequestDto updateUserNavigationDbRequestDto, bool updateIfChanged = true)
  {
    throw new NotImplementedException();
  }

  public Task<bool> DeleteUserNavigationAsync(DeleteUserNavigationDbRequestDto deleteUserNavigationDbRequestDto)
  {
    throw new NotImplementedException();
  }

  public async Task<IReadOnlyList<GetUserNavigationFieldValuesDbResponseDto>> MoveUserNavigationAsync(MoveUserNavigationDbRequestDto moveUserNavigationDbRequestDto, IDbTransaction? transaction = null)
  {
    // Source Navigation을 포함하여 Position에 영향받는 모든 Navigation들의 Id, Parent, Position을 담아서 반환(이 때 반환되는 모든 Navigation의 Parent 속성 값은 모두 일치해야 함).
    List<GetUserNavigationFieldValuesDbResponseDto> resultDtos = new();
    UserNavigationGetFields userNavigationGetField = UserNavigationGetFields.Id | UserNavigationGetFields.Parent | UserNavigationGetFields.Position;

    Guid sourceId = moveUserNavigationDbRequestDto.SourceNavigation.Value;
    Guid targetId = moveUserNavigationDbRequestDto.TargetNavigation.Value;
    NavigationInsertPosition navigationInsertPosition = moveUserNavigationDbRequestDto.NavigationInsertPosition;

    if (sourceId == targetId)
    {
      //todo: MoveNavigationAsync 예외 처리
      throw new ArgumentException("Source와 Target은 동일할 수 없음. 상세 예외 차후 구현");
    }

    AppDbContext context;
    if (transaction is DbTransaction dbTransaction && dbTransaction.DbContext is AppDbContext transactionContext)
    {
      context = transactionContext;
    }

    context = await DbContextFactory.CreateDbContextAsync();

    if (await context.NavigationEntities.FirstOrDefaultAsync(e => e.Id == sourceId) is UserNavigationEntity sourceEntity
      && await context.NavigationEntities.FirstOrDefaultAsync(e => e.Id == targetId) is UserNavigationEntity targetEntity)
    {
      // 이동하려는 타겟의 위치 기준으로 삽입 위치를 결정하는 형제 Entity들을 가져와서,
      // Position 기준 오름차순(Position이 작을수록 컬렉션의 앞에 위치함)으로 정렬
      var siblingEntities = navigationInsertPosition switch
      {
        // 타겟 앞 또는 뒤에 삽입: 타겟과 Parent가 같은 모든 Navigation들
        NavigationInsertPosition.Before or NavigationInsertPosition.After =>
          await context.NavigationEntities
          .Where(e => e.Parent == targetEntity.Parent)
          .OrderBy(e => e.Position)
          .ToListAsync(),
        // 타겟의 첫 번째 혹은 마지막 자식으로 삽입: 타겟을 Parent로 하는 모든 Navigation들
        NavigationInsertPosition.FirstChild or NavigationInsertPosition.LastChild =>
          await context.NavigationEntities
          .Where(e => e.Parent == targetEntity.Id)
          .OrderBy(e => e.Position)
          .ToListAsync(),
        _ => throw new ArgumentException("Invalid NavigationMoveType")
      };

      siblingEntities.Remove(sourceEntity);
      int index = -1;

      // 소스의 Parent와 Position을 조건에 맞게 조정함
      switch (navigationInsertPosition)
      {
        // 타겟 앞에 삽입: 타겟과 Parent는 동일, Position은 타겟보다 1 작음
        case NavigationInsertPosition.Before:
          sourceEntity.Parent = targetEntity.Parent;
          sourceEntity.Position = targetEntity.Position - 1;
          index = siblingEntities.IndexOf(targetEntity);
          break;
        // 타겟 앞에 삽입: 타겟과 Parent는 동일, Position은 타겟보다 1 큼
        case NavigationInsertPosition.After:
          sourceEntity.Parent = targetEntity.Parent;
          sourceEntity.Position = targetEntity.Position + 1;
          index = siblingEntities.IndexOf(targetEntity) + 1;
          break;
        // 타겟의 첫 번째 자식으로 삽입: Parent는 타겟, Position은 기존 첫 요소보다 1 작음, 없으면 0
        case NavigationInsertPosition.FirstChild:
          sourceEntity.Parent = targetEntity.Id;
          sourceEntity.Position = siblingEntities.FirstOrDefault() is UserNavigationEntity firstChild ? firstChild.Position - 1 : 0;
          index = 0;
          break;
        // 타겟의 마지막 자식으로 삽입: Parent는 타겟, Position은 기존 마지막 요소보다 1 큼, 없으면 0
        case NavigationInsertPosition.LastChild:
          sourceEntity.Parent = targetEntity.Id;
          sourceEntity.Position = siblingEntities.LastOrDefault() is UserNavigationEntity lastChild ? lastChild.Position + 1 : 0;
          index = siblingEntities.Count;
          break;
      }

      if (index >= 0)
      {
        siblingEntities.Insert(index, sourceEntity);
      }
      else
      {
        //todo: MoveNavigationAsync 예외 처리
        throw new ArgumentException("나중에 예외 처리 구현");
      }

      // 위치 변경을 완료한 후에 영향받는 Navigation들만 Position을 수정할 수 있도록 repositionEntities 컬렉션 생성 후 위치 재조정
      var repositionEntities = siblingEntities.Select(e => new UserNavigationRepositionEntity(e.Id, e.Position)).ToList();
      var affectedEntities = ReassignPositions(repositionEntities, index);

      // 위치 조정 후 영향받은 엔티티들의 Position들을 반영
      //foreach (var affectedEntity in affectedEntities)
      //{
      //  if (siblingEntities.FirstOrDefault(e => e.Id == affectedEntity.Id) is UserNavigationEntity child)
      //  {
      //    child.Position = affectedEntity.Position;
      //    resultDtos.Add(new GetUserNavigationFieldValuesDbResponseDto()
      //    {
      //      UserNavigationGetFields = userNavigationGetField,
      //      Id = NavigationId.Create(child.Id),
      //      Parent = NavigationId.Create(child.Parent),
      //      Position = child.Position
      //    });
      //  }
      //}

      foreach (var siblingEntity in siblingEntities)
      {
        resultDtos.Add(new GetUserNavigationFieldValuesDbResponseDto()
        {
          UserNavigationGetFields = userNavigationGetField,
          Id = NavigationId.Create(siblingEntity.Id),
          Parent = NavigationId.Create(siblingEntity.Parent),
          Position = siblingEntity.Position
        });
      }
    }

    if (transaction is not null)
    {
      await context.SaveChangesAsync();
      await context.DisposeAsync();
    }

    return resultDtos;
  }

  private class UserNavigationRepositionEntity(Guid id, int position) : IComparable<UserNavigationRepositionEntity>
  {
    public Guid Id { get; init; } = id;
    public int Position { get; set; } = position;

    public int CompareTo(UserNavigationRepositionEntity? other) => other is null ? 1 : this.Position.CompareTo(other.Position);
  }

  private static IReadOnlyList<UserNavigationRepositionEntity> ReassignPositions(IList<UserNavigationRepositionEntity> collection, int index)
  {
    List<UserNavigationRepositionEntity> affected = new();

    int count = collection.Count;

    // index가 허용 범위를 벗어나면 예외 발생(맨 앞/맨 뒤는 허용하지 않음)
    if (index < 1 || index >= count - 1)
    {
      throw new IndexOutOfRangeException();
    }

    int gap = 0;
    int hitIdx;
    int leftIdx, rightIdx;
    int leftPos, rightPos;

    // 중간 위치(midPos) 계산: index 기준 좌우의 Position들의 중간값
    // 오버플로를 방지하는 중간값 계산 방식((A+B)/2 방식은 A+B에서 오버플로 발생 가능성 있음)
    int midPos = collection[index - 1].Position + (collection[index + 1].Position - collection[index - 1].Position) / 2;

    // 좌우로 gap을 늘려가며 적절한 위치(hitIdx)를 탐색
    while (true)
    {
      gap++;
      leftIdx = index - gap;
      rightIdx = index + gap;

      // 왼쪽 끝에 도달하면 hitIdx를 왼쪽 끝으로 설정
      if (leftIdx == -1)
      {
        hitIdx = leftIdx;
        break;
      }
      // 오른쪽 끝에 도달하면 hitIdx를 오른쪽 끝으로 설정
      else if (rightIdx == count)
      {
        hitIdx = rightIdx;
        break;
      }

      leftPos = collection[leftIdx].Position;
      rightPos = collection[rightIdx].Position;

      // midPos와 왼쪽 Position의 차이가 gap 보다 크면 hitIdx를 왼쪽으로 설정
      if (midPos - leftPos >= gap)
      {
        hitIdx = leftIdx;
        break;
      }

      // 오른쪽 Position과 midPos의 차이가 gap보다 크면 hitIdx를 오른쪽으로 설정
      if (rightPos - midPos > gap)
      {
        hitIdx = rightIdx;
        break;
      }
    }

    // gap이 1이면 중간 위치(midPos)로 Position을 할당
    if (gap == 1)
    {
      collection[index].Position = midPos;
    }
    // 왼쪽으로 이동해야 하는 경우
    else if (hitIdx < index)
    {
      collection[index].Position = collection[index - 1].Position;
      // hitIdx+1부터 index-1까지의 엔티티 Position을 1씩 감소 후 영향받은 엔티티에 추가
      for (int i = hitIdx + 1; i < index; i++)
      {
        collection[i].Position--;
        affected.Add(collection[i]);
      }
    }
    // 오른쪽으로 이동해야 하는 경우
    else if (hitIdx > index)
    {
      collection[index].Position = collection[index + 1].Position;
      // index+1부터 hitIdx-1까지의 엔티티 Position을 1씩 증가 후 영향받은 엔티티에 추가
      for (int i = index + 1; i < hitIdx; i++)
      {
        collection[i].Position++;
        affected.Add(collection[i]);
      }
    }

    // 현재 index 위치의 엔티티를 영향받은 엔티티에 추가
    affected.Add(collection[index]);

    return affected;
  }
}
