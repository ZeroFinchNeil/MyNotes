using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteEditorViewModelProvider(IServiceScopeFactory ScopeFactory) : IAsyncViewModelProvider<NoteModel, RichEditTextDocument, NoteEditorViewModel>
{
  public async Task<IAsyncViewModelLease<NoteEditorViewModel>> ResolveAsync(NoteModel note, RichEditTextDocument document)
  {
    var scope = ScopeFactory.CreateAsyncScope();
    try
    {
      return new ViewModelLease()
      {
        ViewModel = ActivatorUtilities.CreateInstance<NoteEditorViewModel>(scope.ServiceProvider, note, document),
        ReleaseFunc = async () =>
        {
          await scope.DisposeAsync();
          return true;
        }
      };
    }
    catch
    {
      await scope.DisposeAsync();
      throw;
    }
  }

  Task<IAsyncViewModelLease<NoteEditorViewModel>?> IAsyncViewModelProvider<NoteModel, RichEditTextDocument, NoteEditorViewModel>.AcquireAsync(NoteModel model) => throw new InvalidOperationException();

  private class ViewModelLease : IAsyncViewModelLease<NoteEditorViewModel>
  {
    public required NoteEditorViewModel ViewModel { get; init; }
    public required Func<Task<bool>> ReleaseFunc { get; init; }

    private bool _disposeStarted;
    private async ValueTask DisposeAsyncCore()
    {
      if (Interlocked.Exchange(ref _disposeStarted, true))
      {
        return;
      }

      if (await ReleaseFunc.Invoke())
      {
        await ViewModel.DisposeAsync();
      }
    }

    public async ValueTask DisposeAsync()
    {
      await DisposeAsyncCore().ConfigureAwait(false);
      GC.SuppressFinalize(this);
    }
  }
}
