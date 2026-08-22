using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Application.Contracts.Converters;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

namespace MyNotes.ViewModels.Notes;

internal partial class NotePreviewViewModel : ViewModelBase, IAsyncDisposable
{
  protected readonly IRtfTextConverter RtfTextConverter;

  protected readonly IAsyncViewModelLease<NoteViewModel> NoteViewModelLease;
  public NoteViewModel NoteViewModel => NoteViewModelLease.ViewModel;
  public NoteModel Note => NoteViewModel.Note;

  private readonly int _previewTextMaxLength = 500;

  public NotePreviewViewModel(IRtfTextConverter rtfTextConverter, IAsyncViewModelLease<NoteViewModel> noteViewModelLease)
  {
    NoteViewModelLease = noteViewModelLease;

    RtfTextConverter = rtfTextConverter;
    _preview = GetPreview();
    RegisterMessengers();
    // TODO: 노트 본문 변경 시 일반 미리보기는 다시 계산하고,
    // 검색 미리보기는 검색 결과와 강조 범위를 함께 재계산할 수 있도록 갱신 흐름을 구현할 것.
  }

  protected bool _disposeStarted;
  protected virtual async ValueTask DisposeAsyncCore()
  {
    if (Interlocked.Exchange(ref _disposeStarted, true))
    {
      return;
    }
    UnregisterMessengers();
    await NoteViewModelLease.DisposeAsync();
  }

  public virtual async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    Dispose(disposing: false);
  }

  protected string _preview = string.Empty;
  public string Preview
  {
    get => _preview;
    set => SetProperty(ref _preview, value);
  }

  private string GetPreview() => RtfTextConverter.GetPreview(Note.Body, 0, _previewTextMaxLength);

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, MessageToken<NoteId>>(this, AppMessageTokens.UpdateNotePreviewToken(Note.Id), (recipient, message) => Preview = GetPreview());
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
