using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Lifetime;
using MyNotes.Debugging;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes.Providers;

internal sealed class NoteViewModelProvider(IServiceProvider serviceProvider) : IAsyncViewModelProvider<NoteModel, NoteViewModel>
{
  private readonly ConcurrentDictionary<NoteId, NoteViewModelCache> ResolveTable = new();
  private readonly Func<NoteModel, NoteViewModelLease> _factory = (noteModel) =>
  {
    AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
    return new NoteViewModelLease(ActivatorUtilities.CreateInstance<NoteViewModel>(scope.ServiceProvider, noteModel), scope);
  };

  public NoteViewModel Resolve(NoteModel noteModel)
  {
    ConsoleHelper.WriteLine(true, "{0}: {1}", "Resolve", true);

    var cache = ResolveTable.GetOrAdd(noteModel.Id, noteId => new NoteViewModelCache());

    cache.SemaphoreSlim.Wait();
    try
    {
      cache.ReferenceCounter ??= new(_factory(noteModel));
      if (cache.ReferenceCounter.TryAcquire(out var viewmodelLease))
      {
        return viewmodelLease.ViewModel;
      }

      NoteViewModelCache newCache = new()
      {
        ReferenceCounter = new ReferenceCounter<NoteViewModelLease>(_factory(noteModel))
      };
      newCache.SemaphoreSlim.Wait();
      try
      {
        ResolveTable.AddOrUpdate(noteModel.Id, newCache, (k, v) => v = newCache);

        return newCache.ReferenceCounter.TryAcquire(out var newViewModelLease) ? newViewModelLease.ViewModel : throw new InvalidOperationException();
      }
      finally
      {
        newCache.SemaphoreSlim.Release();
      }
    }
    finally
    {
      cache.SemaphoreSlim.Release();
    }
  }

  public bool TryResolve(NoteModel noteModel, [NotNullWhen(true)] out NoteViewModel? noteViewModel)
  {
    ConsoleHelper.WriteLine(true, "{0}: {1}", "TryResolve", true);

    NoteId noteId = noteModel.Id;
    if (ResolveTable.TryGetValue(noteId, out var cache)
      && cache.ReferenceCounter is not null)
    {
      cache.SemaphoreSlim.Wait();
      try
      {
        if (cache.ReferenceCounter.TryAcquire(out var viewmodelLease, false))
        {
          if (!viewmodelLease.ViewModel.Disposed)
          {
            noteViewModel = viewmodelLease.ViewModel;
            return true;
          }
          else
          {
            ResolveTable.TryRemove(noteId, out _);
          }
        }
      }
      finally
      {
        cache.SemaphoreSlim.Release();
      }
    }
    noteViewModel = null;
    return false;
  }

  public async Task<bool> ReleaseAsync(NoteModel noteModel)
  {
    ConsoleHelper.WriteLine(true, "{0}: {1}", "ReleaseAsync", true);

    if (ResolveTable.TryGetValue(noteModel.Id, out var cache))
    {
      if (cache.ReferenceCounter is null)
      {
        return false;
      }
      await cache.SemaphoreSlim.WaitAsync();
      try
      {
        if (cache.ReferenceCounter.ReleaseOrDetach(out var viewmodelLease))
        {
          ResolveTable.TryRemove(noteModel.Id, out _);
          await viewmodelLease.ViewModel.DisposeAsync();
          await viewmodelLease.ServiceScope.DisposeAsync();
        }
        return true;
      }
      finally
      {
        cache.SemaphoreSlim.Release();
      }
    }
    return false;
  }

  private sealed record NoteViewModelLease(NoteViewModel ViewModel, AsyncServiceScope ServiceScope);

  private class NoteViewModelCache
  {
    public SemaphoreSlim SemaphoreSlim { get; } = new(1, 1);

    public ReferenceCounter<NoteViewModelLease>? ReferenceCounter { get; set; }
  }
}