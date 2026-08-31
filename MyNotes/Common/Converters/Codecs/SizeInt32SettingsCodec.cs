using MyNotes.Common.Structures;

namespace MyNotes.Common.Converters.Codecs;
internal sealed class SizeInt32SettingsCodec : ISettingsCodec<SizeInt32, Size>
{
  public static SizeInt32SettingsCodec Default => field ??= new();

  private SizeInt32SettingsCodec() { }

  public Size Encode(SizeInt32 settings) => new(settings.Width, settings.Height);

  public SizeInt32 Decode(Size settings) => new((int)Math.Round(settings.Width, 0), (int)Math.Round(settings.Height, 0));
}