using System;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml;

using WinRT.Interop;

namespace MyNotes.Common.Interop;

internal static partial class NativeMethods
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

  #region 싱글 앱 인스턴스
  [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  public static partial IntPtr CreateEvent(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetEvent(IntPtr hEvent);

  [LibraryImport("ole32.dll", SetLastError = true)]
  public static partial uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, [In] IntPtr[] pHandles, out uint dwIndex);

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

  #region System Metrics
  // More info: https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getsystemmetrics
  [LibraryImport("user32.dll", SetLastError = true)]
  public static partial int GetSystemMetrics(int nIndex);
  #endregion
}
