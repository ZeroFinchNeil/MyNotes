using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Services.Updates;

internal sealed record PatchDescriptor<TSource, TKey, TPatch> where TKey : notnull where TPatch : notnull
{
  public required TKey Key { get; init; }
  public required UpdateBatchMode BatchMode { get; init; }
  public required Func<TSource, TPatch> CreatePatch { get; init; }

  public PatchDescriptor() { }

  [SetsRequiredMembers]
  public PatchDescriptor(TKey key, UpdateBatchMode updateBatchMode, Func<TSource, TPatch> createPatch)
  {
    Key = key;
    BatchMode = updateBatchMode;
    CreatePatch = createPatch;
  }
}