using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteEditorViewModelProvider(IServiceScopeFactory ScopeFactory) : IAsyncViewModelProvider<NoteModel, NoteEditorViewModel>
{
  private readonly Dictionary<NoteId, NoteEditorViewModelScope> ResolvedViewModels = new();

  NoteEditorViewModel IAsyncViewModelProvider<NoteModel, NoteEditorViewModel>.Resolve(NoteModel note) => throw new NotImplementedException();

  public async ValueTask<NoteEditorViewModel> Resolve(NoteModel note, RichEditTextDocument document)
  {
    if (TryResolve(note, out var viewmodel))
    {
      return viewmodel;
    }

    var scope = ScopeFactory.CreateAsyncScope();
    try
    {
      NoteEditorViewModel newViewModel = ActivatorUtilities.CreateInstance<NoteEditorViewModel>(scope.ServiceProvider, note, document);
      ResolvedViewModels[note.Id] = new NoteEditorViewModelScope(newViewModel, scope);

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
    if (ResolvedViewModels.TryGetValue(note.Id, out var viewmodelScope)
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
        await ResolvedViewModels[note.Id].Scope.DisposeAsync();
      }

      return ResolvedViewModels.Remove(note.Id);
    }

    return false;
  }

  private record NoteEditorViewModelScope(NoteEditorViewModel ViewModel, AsyncServiceScope Scope);
}
