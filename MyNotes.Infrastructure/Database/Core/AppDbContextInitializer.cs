using Microsoft.EntityFrameworkCore;

namespace MyNotes.Infrastructure.Database.Core;

/// <summary>
/// EFCore DB Context를 초기화합니다.
/// </summary>
/// <remarks>
/// 애플리케이션 시작 시 애플리케이션 컨텍스트에 사용되는 기본 데이터베이스가 존재하는지 확인하기 위해 사용됩니다.
/// 이 클래스는 시작 시 한 번만 인스턴스화해야 하며, 데이터베이스 스키마 또는 구성이 변경되지 않는 한 반복적인 초기화는 필요하지 않습니다.
/// </remarks>
internal sealed class AppDbContextInitializer
{
  public AppDbContextInitializer(IDbContextFactory<AppDbContext> factory)
  {
    using var context = factory.CreateDbContext();
    context.Database.EnsureCreated();
  }
}
