namespace MyNotes.Helpers;

internal static class NumericHelper
{
  public static SizeInt32 ToSizeInt32(Size size) => new((int)size.Width, (int)size.Height);
  public static SizeInt32 ToSizeInt32Rounded(Size size) => new((int)Math.Round(size.Width), (int)Math.Round(size.Height));
  public static Size ToSize(SizeInt32 size) => new(size.Width, size.Height);

  public static PointInt32 ToPointInt32(Point point) => new((int)point.X, (int)point.Y);
  public static PointInt32 ToPointInt32Rounded(Point point) => new((int)Math.Round(point.X), (int)Math.Round(point.Y));
  public static Point ToPoint(PointInt32 point) => new(point.X, point.Y);

  public static RectInt32 ToRectInt32(Rect rect) => new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
  public static RectInt32 ToRectInt32Rounded(Rect rect) => new((int)Math.Round(rect.X), (int)Math.Round(rect.Y), (int)Math.Round(rect.Width), (int)Math.Round(rect.Height));
  public static Rect ToRect(RectInt32 rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

  // Extensions
  // 확장 메서드 및 확장 멤버(정적/인스턴스 모두 가능) 정의
  extension(Size size)
  {
    public SizeInt32 SizeInt32 => new((int)size.Width, (int)size.Height);
    public SizeInt32 RoundedSizeInt32 => new((int)Math.Round(size.Width), (int)Math.Round(size.Height));

    public SizeInt32 ToScaledSizeInt32(double scale, bool isRounded = false) => isRounded
      ? new((int)Math.Round(size.Width * scale), (int)Math.Round(size.Height * scale))
      : new((int)(size.Width * scale), (int)(size.Height * scale));
  }

  extension(SizeInt32 size)
  {
    public Size Size => new(size.Width, size.Height);
  }

  extension(Point point)
  {
    public PointInt32 PointInt32 => new((int)point.X, (int)point.Y);
    public PointInt32 RoundedPointInt32 => new((int)Math.Round(point.X), (int)Math.Round(point.Y));

    public PointInt32 ToScaledPointInt32(double scale, bool isRounded = false) => isRounded
      ? new((int)Math.Round(point.X * scale), (int)Math.Round(point.Y * scale))
      : new((int)(point.X * scale), (int)(point.Y * scale));
  }

  extension(PointInt32 point)
  {
    public Point Point => new(point.X, point.Y);
  }

  extension(Rect rect)
  {
    public RectInt32 RectInt32 => new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    public RectInt32 RoundedRectInt32 => new(
      _X: (int)Math.Round(rect.X), _Y: (int)Math.Round(rect.Y),
      _Width: (int)Math.Round(rect.Width), _Height: (int)Math.Round(rect.Height));

    public RectInt32 ToScaledRectInt32(double scale, bool isRounded = false) => isRounded
      ? new(
        _X: (int)Math.Round(rect.X * scale), _Y: (int)Math.Round(rect.Y * scale),
        _Width: (int)Math.Round(rect.Width * scale), _Height: (int)Math.Round(rect.Height * scale))
      : new((int)(rect.X * scale), (int)(rect.Y * scale), (int)(rect.Width * scale), (int)(rect.Height * scale));
  }

  extension(RectInt32 rect)
  {
    public Rect Rect => new(rect.X, rect.Y, rect.Width, rect.Height);
  }
}
