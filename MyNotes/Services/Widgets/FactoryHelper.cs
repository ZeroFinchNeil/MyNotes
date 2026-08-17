using System.Runtime.InteropServices;

namespace MyNotes.Services.Widgets;

public static class Guids
{
  public const string IClassFactory = "00000001-0000-0000-C000-000000000046";
  public const string IUnknown = "00000000-0000-0000-C000-000000000046";
}

#pragma warning disable SYSLIB1096 // 'GeneratedComInterface'로 변환
[ComImport, ComVisible(false), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(Guids.IClassFactory)]
public interface IClassFactory
{
  [PreserveSig]
  int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);
  [PreserveSig]
  int LockServer(bool fLock);
}
