using MyNotes.Application.Contracts.Converters;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteSearchPreviewViewModel : NotePreviewViewModel, IAsyncDisposable
{
  public NoteSearchPreviewViewModel(IRtfTextConverter rtfTextConverter, IAsyncViewModelLease<NoteViewModel> noteViewModelLease) : base(rtfTextConverter, noteViewModelLease)
  {

  }

  protected override async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }

    await NoteViewModelLease.DisposeAsync();
  }

  public override async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    Dispose(disposing: false);
  }

  public void HighlightPreview(IReadOnlyList<Range> highlightRange, string color = "#FF03FCD3") => RtfTextConverter.Highlight(ref _preview, highlightRange, color);
}