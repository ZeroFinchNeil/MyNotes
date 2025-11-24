using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml;

using WinRT.Interop;

namespace MyNotes.Common.Interop;

public static partial class NativeMethods
{
  #region 창 관련
  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetClientRect(IntPtr hWnd, out RECT lpRect);

  [LibraryImport("user32.dll", SetLastError = true)]
  public static partial uint GetDpiForWindow(IntPtr hWnd);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

  [StructLayout(LayoutKind.Sequential)]
  public struct WINDOWINFO
  {
    public int cbSize = Marshal.SizeOf<WINDOWINFO>();
    public RECT rcWindow;
    public RECT rcClient;
    public int dwStyle;
    public int dwExStyle;
    public int dwWindowStatus;
    public uint cxWindowBorders;
    public uint cyWindowBorders;
    public ushort atomWindowType;
    public ushort wCreatorVersion;

    public WINDOWINFO() { }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct RECT
  {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }

  public static IntPtr GetWindowHandle(Window window) => WindowNative.GetWindowHandle(window);

  public static double GetWindowScaleFactor(IntPtr hWnd) => GetDpiForWindow(hWnd) / 96.0;
  #endregion

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

  #region 싱글 앱 인스턴스
  [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  public static partial IntPtr CreateEvent(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetEvent(IntPtr hEvent);

  [LibraryImport("ole32.dll", SetLastError = true)]
  public static partial uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, IntPtr[] pHandles, out uint dwIndex);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetForegroundWindow(IntPtr hWnd);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
  #endregion

  #region 메시지 루프 관련
  [LibraryImport("user32.dll", EntryPoint = "GetMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

  [LibraryImport("user32.dll", EntryPoint = "PostThreadMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool PostThreadMessage(uint idThread, uint Msg, UIntPtr wParam, IntPtr lParam);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  public static partial uint GetCurrentThreadId();

  [LibraryImport("user32.dll", SetLastError = true)]
  public static partial void PostQuitMessage(int nExitCode);

  public enum WindowMessage : uint
  {
    WM_QUIT = 0x0012
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct MSG
  {
    public IntPtr hwnd;
    public uint message;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public int pt_x;
    public int pt_y;
  }
  #endregion

  #region Job 핸들 관련
  [LibraryImport("kernel32.dll", EntryPoint = "CreateJobObjectW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  public static partial IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

  [LibraryImport("kernel32.dll", EntryPoint = "OpenJobObjectW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  public static partial IntPtr OpenJobObject(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

  public const int JobObjectExtendedLimitInformation = 9;
  public const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;

  [StructLayout(LayoutKind.Sequential)]
  public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
  {
    public long PerProcessUserTimeLimit;
    public long PerJobUserTimeLimit;
    public uint LimitFlags;
    public UIntPtr MinimumWorkingSetSize;
    public UIntPtr MaximumWorkingSetSize;
    public uint ActiveProcessLimit;
    public IntPtr Affinity;
    public uint PriorityClass;
    public uint SchedulingClass;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct IO_COUNTERS
  {
    public ulong ReadOperationCount;
    public ulong WriteOperationCount;
    public ulong OtherOperationCount;
    public ulong ReadTransferCount;
    public ulong WriteTransferCount;
    public ulong OtherTransferCount;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
  {
    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
    public IO_COUNTERS IoInfo;
    public UIntPtr ProcessMemoryLimit;
    public UIntPtr JobMemoryLimit;
    public UIntPtr PeakProcessMemoryUsed;
    public UIntPtr PeakJobMemoryUsed;
  }

  public static void CreateJob(IntPtr hJob)
  {
    JOBOBJECT_BASIC_LIMIT_INFORMATION basicLimitInfo = new() { LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE };
    JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimitInfo = new() { BasicLimitInformation = basicLimitInfo };

    int length = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
    IntPtr pInfo = Marshal.AllocHGlobal(length);
    Marshal.StructureToPtr(extendedLimitInfo, pInfo, false);

    SetInformationJobObject(hJob, JobObjectExtendedLimitInformation, pInfo, (uint)length);
    Marshal.FreeHGlobal(pInfo);
  }
  #endregion

  #region COM OLE 관련
  [DllImport("ole32.dll", SetLastError = true)]
  public static extern int CoRegisterClassObject([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [MarshalAs(UnmanagedType.IUnknown)] object pUnk, uint dwClsContext, uint flags, out uint lpdwRegister);

  [DllImport("ole32.dll", SetLastError = true)]
  public static extern int CoGetClassObject([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, uint dwClsContext, IntPtr pvReserved, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppv);

  [LibraryImport("ole32.dll", SetLastError = true)]
  public static partial int CoRevokeClassObject(uint dwRegister);
  #endregion

  #region 콘솔 창 관련
  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool AllocConsole();

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool FreeConsole();

  [LibraryImport("kernel32.dll", SetLastError = true)]
  private static partial IntPtr GetStdHandle(int nStdHandle);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

  [LibraryImport("kernel32.dll")]
  public static partial IntPtr GetConsoleWindow();

  public static void SetConsole()
  {
    AllocConsole();
    IntPtr consoleHwnd = GetConsoleWindow();
    if (consoleHwnd == IntPtr.Zero)
      return;

    uint SWP_NOMOVE = 0x0002;
    uint SWP_NOSIZE = 0x0001;
    uint SWP_SHOWWINDOW = 0x0040;
    SetWindowPos(consoleHwnd, new IntPtr(-1), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

    int STD_INPUT_HANDLE = -10;
    uint ENABLE_EXTENDED_FLAGS = 0x0080;
    uint ENABLE_QUICK_EDIT_MODE = 0x0040;

    var handle = GetStdHandle(STD_INPUT_HANDLE);

    if (GetConsoleMode(handle, out uint mode))
    {
      mode &= ~ENABLE_QUICK_EDIT_MODE;
      mode |= ENABLE_EXTENDED_FLAGS;
      SetConsoleMode(handle, mode);
    }

    Console.BackgroundColor = ConsoleColor.White;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.Clear();
  }
  #endregion

  #region System Metrics
  // More info: https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
  [LibraryImport("user32.dll", SetLastError = true)]
  public static partial int GetSystemMetrics(int nIndex);
  #endregion
}
