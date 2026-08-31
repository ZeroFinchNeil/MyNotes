using MyNotes.Common.Structures;

namespace MyNotes.Common.Converters.Codecs;

internal sealed class PointInt32SettingsCodec : ISettingsCodec<PointInt32, Point>
{
  public static PointInt32SettingsCodec Default => field ??= new();

  private PointInt32SettingsCodec() { }

  public Point Encode(PointInt32 settings) => new(settings.X, settings.Y);

  public PointInt32 Decode(Point settings) => new((int)Math.Round(settings.X, 0), (int)Math.Round(settings.Y, 0));
}