using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MyNotes.Common.Interop;

internal static partial class NativeMethods
{
  #region 모니터(디스플레이) 관련
  [LibraryImport("user32.dll", SetLastError = true)]
  public static partial IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

  [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

  [LibraryImport("user32.dll", EntryPoint = "EnumDisplaySettingsExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool EnumDisplaySettingsEx(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode, uint dwFlags);

  public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct DEVMODE
  {
    private unsafe fixed char _dmDeviceName[32];

    public ushort dmSpecVersion;
    public ushort dmDriverVersion;
    public ushort dmSize = (ushort)Marshal.SizeOf<DEVMODE>();
    public ushort dmDriverExtra;
    public uint dmFields;

    public int dmPositionX;
    public int dmPositionY;
    public uint dmDisplayOrientation;
    public uint dmDisplayFixedOutput;

    public short dmColor;
    public short dmDuplex;
    public short dmYResolution;
    public short dmTTOption;
    public short dmCollate;

    private unsafe fixed char _dmFormName[32];

    public ushort dmLogPixels;
    public uint dmBitsPerPel;
    public uint dmPelsWidth;
    public uint dmPelsHeight;
    public uint dmDisplayFlags;
    public uint dmDisplayFrequency;
    public uint dmICMMethod;
    public uint dmICMIntent;
    public uint dmMediaType;
    public uint dmDitherType;
    public uint dmReserved1;
    public uint dmReserved2;
    public uint dmPanningWidth;
    public uint dmPanningHeight;

    public unsafe string dmDeviceName
    {
      get
      {
        fixed (char* p = _dmDeviceName)
          return new string(p);
      }
    }

    public unsafe string dmFormName
    {
      get
      {
        fixed (char* p = _dmFormName)
          return new string(p);
      }
    }

    public DEVMODE() { }
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct MONITORINFOEX
  {
    public int cbSize = Marshal.SizeOf<MONITORINFOEX>();
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
    private unsafe fixed char _szDevice[32];

    public unsafe string szDevice
    {
      get
      {
        fixed (char* p = _szDevice)
          return new string(p);
      }
    }

    public MONITORINFOEX() { }
  }

  public static MONITORINFOEX? GetMonitorInfoForWindow(IntPtr hWnd)
  {
    if (!GetWindowRect(hWnd, out var rect))
      return null;

    //uint MONITOR_DEFAULTTONULL = 0x00000000;
    //uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
    uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    IntPtr hMonitor = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);

    MONITORINFOEX monitorInfo = new();
    return GetMonitorInfo(hMonitor, ref monitorInfo) ? monitorInfo : null;
  }

  public static IReadOnlyList<MONITORINFOEX> GetActiveMonitorsInfo()
  {
    List<MONITORINFOEX> monitors = new();

    EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMonitor, hdcMonitor, ref lprcMonitor, dwData) =>
    {
      MONITORINFOEX monitorInfo = new();

      if (GetMonitorInfo(hMonitor, ref monitorInfo))
        monitors.Add(monitorInfo);
      return true;
    }, IntPtr.Zero);

    return monitors;
  }

  public static IReadOnlyList<(MONITORINFOEX Info, DEVMODE DevMode)> GetActiveMonitorsInfoEx()
  {
    List<(MONITORINFOEX, DEVMODE)> monitors = new();
    uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFF;

    EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMonitor, hdcMonitor, ref lprcMonitor, dwData) =>
    {
      MONITORINFOEX monitorInfo = new();

      if (GetMonitorInfo(hMonitor, ref monitorInfo))
      {
        DEVMODE devMode = new();
        EnumDisplaySettingsEx(monitorInfo.szDevice, (int)ENUM_CURRENT_SETTINGS, ref devMode, 0);
        monitors.Add((monitorInfo, devMode));
      }
      return true;
    }, IntPtr.Zero);

    return monitors;
  }
  #endregion
}
