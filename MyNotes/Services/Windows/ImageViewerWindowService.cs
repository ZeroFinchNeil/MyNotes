using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Content;

using MyNotes.Models.Media;
using MyNotes.Shell.Contracts.Windowing;
using MyNotes.Views.Windows;

using WinRT.Interop;

namespace MyNotes.Services.Windows;

internal class ImageViewerWindowService : IWindowService<ImageViewerWindow>
{
  private readonly INativeWindowing NativeWindowing;
  private KeyValuePair<ImageCollectionKey, WeakReference<ImageViewerWindow>>? _imageViewerWindowPair;

  public ImageViewerWindowService(INativeWindowing nativeWindowing)
  {
    NativeWindowing = nativeWindowing;
  }

  public async Task<ImageViewerWindow> GetOrCreate(ImageCollectionKey collectionKey, ImageDescriptor? selection)
  {
    if (_imageViewerWindowPair is not null)
    {
      var pair = _imageViewerWindowPair.Value;
      if (pair.Value.TryGetTarget(out var imageViewerWindow))
      {
        if (pair.Key == collectionKey && !imageViewerWindow.IsClosed)
        {
          imageViewerWindow.ChangeImageSelection(selection);
          return imageViewerWindow;
        }
        else
        {
          imageViewerWindow.Close();
        }
      }
    }

    ImageViewerWindow newWindow = new(collectionKey);
    newWindow.ChangeImageSelection(selection);

    _imageViewerWindowPair = new KeyValuePair<ImageCollectionKey, WeakReference<ImageViewerWindow>>(collectionKey, new(newWindow));
    return newWindow;
  }

  public bool TryGetCurrentWindow([NotNullWhen(true)] out ImageViewerWindow? imageViewerWindow)
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

  public bool TryGetWindowInfo(FrameworkElement element, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
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
      else if (TryGetCurrentWindow(out var imageViewerWindow))
      {
        hWnd = WindowNative.GetWindowHandle(imageViewerWindow);
        appWindow = imageViewerWindow.AppWindow;
      }
    }
    catch (Exception e)
    {
      throw new NotImplementedException("이미지 로딩 오류 시 예외 구현", e);
      //LoggingService.Write(e);
    }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

  public bool TryExecuteOnWindow(Action<ImageViewerWindow> action)
  {
    if (TryGetCurrentWindow(out var imageViewerWindow))
    {
      action.Invoke(imageViewerWindow);
      return true;
    }
    return false;
  }
}
