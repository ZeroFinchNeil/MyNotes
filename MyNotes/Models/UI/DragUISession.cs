using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Models.UI;

[Debugging.ReferenceTracker]
internal sealed partial class DragUISession : IDisposable
{
  public DragUISession() { TrackReference(); }

  public required string FormatId
  {
    get => Disposed ? string.Empty : field;
    init;
  }

  public object? DataView
  {
    get => Disposed ? null : field;
    set;
  }

  public required DataPackageOperation DataPackageOperation
  {
    get => Disposed ? DataPackageOperation.None : field;
    init;
  }

  public string? DragUIOverrideCaption
  {
    get => Disposed ? string.Empty : field;
    set;
  }

  public bool IsExpired => Disposed;

  public bool Disposed { get; private set; }

  private void Dispose(bool disposing)
  {
    if (!Disposed)
    {
      if (disposing)
      {

      }

      Disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}