namespace MyNotes.Helpers;

internal static class IconHelper
{
  public static Uri GetUri(string icon) => new Uri($"ms-appx:///Assets/Icons/FluentEmoji/{icon}");
}
