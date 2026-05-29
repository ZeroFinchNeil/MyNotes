using Microsoft.EntityFrameworkCore;

namespace MyNotes.Application.Contracts.Database.Core;

internal interface IAppDbTransactionContext
{
  public DbContext DbContext { get; }
}

// Application 계층 규칙:
// - IAppDbTransactionContext는 Repository에 트랜잭션 범위를 전달하기 위한 계약으로만 사용한다.
// - Application Service는 DbContext를 직접 조회/저장/생성하지 않는다.
// - DbContext 직접 조작은 Infrastructure Repository 구현체에서만 수행한다.