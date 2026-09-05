using MyNotes.Application.Contracts.Converters;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class NoteSearchPreviewViewModel(IRtfTextConverter rtfTextConverter, IAsyncViewModelLease<NoteViewModel> noteViewModelLease) : NotePreviewViewModel(rtfTextConverter, noteViewModelLease)
{
  public void HighlightPreview(IReadOnlyList<Range> highlightRange, string color = "#FF03FCD3") => RtfTextConverter.Highlight(ref _preview, highlightRange, color);
}