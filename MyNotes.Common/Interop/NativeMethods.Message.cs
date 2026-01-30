using System;
using System.Runtime.InteropServices;

namespace MyNotes.Common.Interop;

internal static partial class NativeMethods
{
  #region 메시지 루프 관련
  [LibraryImport("user32.dll", EntryPoint = "GetMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

  // 특정 스레드에 메시지를 비동기적으로 큐에 넣음(hWnd 없이 가능)
  [LibraryImport("user32.dll", EntryPoint = "PostThreadMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

  [LibraryImport("kernel32.dll", SetLastError = true)]
  public static partial uint GetCurrentThreadId();

  // 현재 메시지 루프에 WM_QUIT 메시지를 넣어 루프 종료를 알림
  [LibraryImport("user32.dll", SetLastError = true)]
  public static partial void PostQuitMessage(int nExitCode);

  // 지정 hWnd에 메시지를 동기적으로 즉시 전달, 함수가 반환될 때까지 메시지 처리가 완료됨
  [LibraryImport("user32.dll", EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  public static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

  // 지정 hWnd에 메시지를 비동기적으로 메시지 큐에 넣음
  [LibraryImport("user32.dll", EntryPoint = "PostMessageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

  public enum WindowMessage : uint
  {
    WM_CLOSE = 0x0010,
    WM_QUERYENDSESSION = 0x0011,
    WM_QUIT = 0x0012,
    WM_ENDSESSION = 0x0016,
    WM_SYSCOMMAND = 0x0112
  }

  public enum SystemCommand : uint
  {
    SC_CLOSE = 0xF060,
    SC_MAXIMIZE = 0xF030,
    SC_MINIMIZE = 0xF020
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
}
