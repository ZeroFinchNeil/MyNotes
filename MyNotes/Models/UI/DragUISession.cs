using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Models.UI;

internal class DragUISession : IDisposable
{
  public required string FormatId
  {
    get => _disposed ? string.Empty : field;
    init;
  }

  public object? DataView
  {
    get => _disposed ? null : field;
    set;
  }

  public required DataPackageOperation DataPackageOperation
  {
    get => _disposed ? DataPackageOperation.None : field;
    init;
  }

  public string? DragUIOverrideCaption
  {
    get => _disposed ? string.Empty : field;
    set;
  }

  public bool IsExpired => _disposed;
  public bool IsDisposed => _disposed;

  private bool _disposed;
  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {

      }

      _disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}