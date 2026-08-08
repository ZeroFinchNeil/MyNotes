using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Services.Updates;

internal sealed record PatchDescriptor<TModel, TPatch> where TPatch : notnull
{
  public required string PropertyName { get; init; }
  public required UpdateDispatchMode UpdateDispatchMode { get; init; }
  public required Func<TModel, TPatch> CreatePatch { get; init; }

  public PatchDescriptor() { }

  [SetsRequiredMembers]
  public PatchDescriptor(string propertyName, UpdateDispatchMode updateDispatchMode, Func<TModel, TPatch> createPatch)
  {
    PropertyName = propertyName;
    UpdateDispatchMode = updateDispatchMode;
    CreatePatch = createPatch;
  }
}