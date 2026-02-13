using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Models.UI;

internal class DragUISession : IDisposable
{
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

  public bool Disposed { get; protected set; }

  protected virtual void Dispose(bool disposing)
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