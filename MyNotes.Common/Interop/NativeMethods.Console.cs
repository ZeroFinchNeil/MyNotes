using System;
using System.Runtime.InteropServices;

namespace MyNotes.Common.Interop;

internal static partial class NativeMethods
{
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

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, ref CONSOLE_FONT_INFOEX consoleCurrentFontEx);

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  private struct CONSOLE_FONT_INFOEX
  {
    public uint cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>();
    public uint nFont;
    public COORD dwFontSize;
    public int FontFamily;
    public int FontWeight;

    private unsafe fixed char _faceName[32];
    public unsafe string FaceName
    {
      get
      {
        fixed (char* p = _faceName)
          return new string(p);
      }
      set
      {
        fixed (char* p = _faceName)
        {
          for (int i = 0; i < 32; i++)
            p[i] = '\0';

          int len = Math.Min(31, value.Length);
          for (int i = 0; i < len; i++)
            p[i] = value[i];
        }
      }
    }

    public CONSOLE_FONT_INFOEX() { }
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct COORD
  {
    public short X;
    public short Y;
  }

  public static void SetConsole()
  {
    AllocConsole();
    IntPtr consoleHwnd = GetConsoleWindow();
    if (consoleHwnd == IntPtr.Zero)
      return;

    //uint SWP_NOMOVE = 0x0002;
    //uint SWP_NOSIZE = 0x0001;
    //uint SWP_SHOWWINDOW = 0x0040;
    //SetWindowPos(consoleHwnd, new IntPtr(-1), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

    SetConsoleFont("Cascadia Mono");

    int STD_INPUT_HANDLE = -10;
    uint ENABLE_EXTENDED_FLAGS = 0x0080;
    uint ENABLE_MOUSE_INPUT = 0x0010;
    //uint ENABLE_QUICK_EDIT_MODE = 0x0040;

    var handle = GetStdHandle(STD_INPUT_HANDLE);

    if (GetConsoleMode(handle, out uint mode))
    {
      //mode &= ~ENABLE_QUICK_EDIT_MODE;
      mode |= ENABLE_MOUSE_INPUT;
      mode |= ENABLE_EXTENDED_FLAGS;
      SetConsoleMode(handle, mode);
    }

    Console.BackgroundColor = ConsoleColor.White;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.Clear();
  }

  public static void SetConsoleFont(string fontName, short width = 0, short height = 16)
  {
    int STD_OUTPUT_HANDLE = -11;
    IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);

    var info = new CONSOLE_FONT_INFOEX
    {
      FaceName = fontName,
      dwFontSize = new COORD { X = width, Y = height },
      FontFamily = 0x36,
      FontWeight = 400
    };

    SetCurrentConsoleFontEx(hnd, false, ref info);
  }
  #endregion
}
