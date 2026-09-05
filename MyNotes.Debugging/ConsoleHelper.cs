using System;

namespace MyNotes.Debugging;

public static class ConsoleHelper
{
  public static void WriteLine(bool visible)
  {
    if (visible)
    {
      Console.WriteLine();
      //Debug.WriteLine("");
    }
  }

  public static void WriteLine(bool visible, string text)
  {
    if (visible)
    {
      Console.WriteLine(text);
      //Debug.WriteLine(text);
    }
  }

  public static void WriteLine(bool visible, string format, params ReadOnlySpan<object?> objects)
  {
    if (visible)
    {
      var line = string.Format(format, objects);
      Console.WriteLine(line);
      //Debug.WriteLine(line);
    }
  }
}