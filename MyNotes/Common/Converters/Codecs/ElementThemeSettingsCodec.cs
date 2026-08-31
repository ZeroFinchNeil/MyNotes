using MyNotes.Common.Structures;

namespace MyNotes.Common.Converters.Codecs;

internal sealed class ElementThemeSettingsCodec : ISettingsCodec<ElementTheme, int>
{
  public static ElementThemeSettingsCodec Default => field ??= new();
  private ElementThemeSettingsCodec() { }

  public int Encode(ElementTheme input) => (int)input;

  public ElementTheme Decode(int output) => (ElementTheme)output;
}