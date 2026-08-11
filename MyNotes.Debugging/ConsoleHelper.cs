using System;

namespace MyNotes.Debugging;

public static class ConsoleHelper
{
  public static void WriteLine(bool visible)
  {
    if (visible)
    {
      Console.WriteLine();
    }
  }

  public static void WriteLine(bool visible, string format, params ReadOnlySpan<object> objects)
  {
    if (visible)
    {
      Console.WriteLine(format, objects);
    }
  }
}