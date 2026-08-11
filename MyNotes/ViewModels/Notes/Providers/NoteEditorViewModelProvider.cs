using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteEditorViewModelProvider(IServiceScopeFactory ScopeFactory) : IAsyncViewModelProvider<NoteModel, NoteEditorViewModel>
{
  private readonly Dictionary<NoteId, NoteEditorViewModelScope> ResolveTable = new();

  NoteEditorViewModel IAsyncViewModelProvider<NoteModel, NoteEditorViewModel>.Resolve(NoteModel note) => throw new NotImplementedException();

  public async ValueTask<NoteEditorViewModel> ResolveAsync(NoteModel note, RichEditTextDocument document)
  {
    if (TryResolve(note, out var viewmodel))
    {
      return viewmodel;
    }

    var scope = ScopeFactory.CreateAsyncScope();
    try
    {
      NoteEditorViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteEditorViewModel>(scope.ServiceProvider, note, document);
      ResolveTable[note.Id] = new NoteEditorViewModelScope(newViewModel, scope);

      return newViewModel;
    }
    catch
    {
      await scope.DisposeAsync();
      throw;
    }
  }

  public bool TryResolve(NoteModel note, [NotNullWhen(true)] out NoteEditorViewModel? noteEditorViewModel)
  {
    if (ResolveTable.TryGetValue(note.Id, out var viewmodelScope)
        && !viewmodelScope.ViewModel.Disposed)
    {
      noteEditorViewModel = viewmodelScope.ViewModel;
      return true;
    }

    noteEditorViewModel = null;
    return false;
  }

  public async Task<bool> ReleaseAsync(NoteModel note)
  {
    if (TryResolve(note, out var viewmodel))
    {
      if (!viewmodel.Disposed)
      {
        await viewmodel.DisposeAsync();
        await ResolveTable[note.Id].Scope.DisposeAsync();
      }

      return ResolveTable.Remove(note.Id);
    }

    return false;
  }

  private record NoteEditorViewModelScope(NoteEditorViewModel ViewModel, AsyncServiceScope Scope);
}
