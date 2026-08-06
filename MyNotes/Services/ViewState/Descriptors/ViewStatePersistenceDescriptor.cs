using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Services.ViewState.Descriptors;

internal sealed record ViewStatePersistenceDescriptor<TModel, TPatch>
{
  public required string PropertyName { get; init; }
  public required ViewStateSaveMode SaveMode { get; init; }
  public required Func<TModel, TPatch> CreatePatch { get; init; }

  public ViewStatePersistenceDescriptor() { }

  [SetsRequiredMembers]
  public ViewStatePersistenceDescriptor(string propertyName, ViewStateSaveMode saveMode, Func<TModel, TPatch> createPatch)
  {
    PropertyName = propertyName;
    SaveMode = saveMode;
    CreatePatch = createPatch;
  }
}