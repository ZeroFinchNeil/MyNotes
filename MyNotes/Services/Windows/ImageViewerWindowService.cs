using MyNotes.Application.Contracts.Windows;

namespace MyNotes.Services.Windows;

internal class ImageViewerWindowService : WindowService, IImageViewerWindowService
{
  private KeyValuePair<ImageCollectionKey, WeakReference<ImageViewerWindow>>? _imageViewerWindowPair;

  public async Task<ImageViewerWindow> GetOrCreateImageViewerWindow(ImageCollectionKey key)
  {
    if (_imageViewerWindowPair is not null)
    {
      var pair = _imageViewerWindowPair.Value;
      if (pair.Value.TryGetTarget(out var imageViewerWindow))
      {
        if (pair.Key == key && !imageViewerWindow.IsClosed)
        {
          return imageViewerWindow;
        }
        else
        {
          imageViewerWindow.Close();
        }
      }
    }

    ImageViewerWindow newWindow = new(key);
    _imageViewerWindowPair = new KeyValuePair<ImageCollectionKey, WeakReference<ImageViewerWindow>>(key, new(newWindow));
    return newWindow;
  }

  public bool TryGetCurrentImageViewerWindow([NotNullWhen(true)] out ImageViewerWindow? imageViewerWindow)
  {
    imageViewerWindow = null;

    if (_imageViewerWindowPair is not null)
    {
      var pair = _imageViewerWindowPair.Value;
      if (pair.Value.TryGetTarget(out var w) && !w.IsClosed)
      {
        imageViewerWindow = w;
        return true;
      }
    }

    return false;
  }

  public bool TryGetImageViewerWindowInfo(FrameworkElement element, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
  {
    hWnd = IntPtr.Zero;
    appWindow = null;

    try
    {
      if (element.XamlRoot is XamlRoot xamlRoot
        && xamlRoot.ContentIslandEnvironment is ContentIslandEnvironment env)
      {
        var windowId = env.AppWindowId;
        hWnd = Win32Interop.GetWindowFromWindowId(windowId);
        appWindow = AppWindow.GetFromWindowId(windowId);
      }
      else if (TryGetCurrentImageViewerWindow(out var imageViewerWindow))
      {
        hWnd = WindowNative.GetWindowHandle(imageViewerWindow);
        appWindow = imageViewerWindow.AppWindow;
      }
    }
    catch (Exception e)
    {
      LoggingService.Write(e);
    }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

  public bool TryExecuteOnImageViewerWindow(Action<ImageViewerWindow> action)
  {
    if (TryGetCurrentImageViewerWindow(out var imageViewerWindow))
    {
      action.Invoke(imageViewerWindow);
      return true;
    }
    return false;
  }

}
