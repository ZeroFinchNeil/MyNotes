using System;

namespace MyNotes.Infrastructure.Logging.Entities;

internal record ExceptionLogEntry : ILogEntity<ExceptionLogEntry>
{
  public required DateTimeOffset Time { get; init; }
  public required int HResult { get; init; }
  public required string Message { get; init; }
  public required string? Source { get; init; }
  public required string? StackTrace { get; init; }
  public required string? TargetSiteName { get; init; }
  public required string? TargetSiteReflectedType { get; init; }
  public required string? TargetSiteAssembly { get; init; }
}
