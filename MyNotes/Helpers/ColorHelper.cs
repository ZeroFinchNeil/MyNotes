namespace MyNotes.Helpers;

internal static class ColorHelper
{
  public static SolidColorBrush ToBrush(Color c) => new SolidColorBrush(c);
  public static AcrylicBrush ToAcrylicBrush(Color c, double tintOpacity = 1.0, double tintLuminosityOpacity = 1.0) => new AcrylicBrush() { FallbackColor = c, TintColor = c, TintOpacity = tintOpacity, TintLuminosityOpacity = tintLuminosityOpacity };

  public const double WCAG_AA_Normal = 4.5;
  public const double WCAG_AA_Large = 3.0;
  public const double WCAG_AAA_Narmal = 7.0;
  public const double WCAG_AAA_Large = 4.5;

  public static double GetRelativeLuminance(Color c)
  {
    var rSRGB = c.R / 255.0;
    var gSRGB = c.G / 255.0;
    var bSRGB = c.B / 255.0;

    var r = rSRGB <= 0.04045 ? rSRGB / 12.92 : Math.Pow((rSRGB + 0.055) / 1.055, 2.4);
    var g = gSRGB <= 0.04045 ? gSRGB / 12.92 : Math.Pow((gSRGB + 0.055) / 1.055, 2.4);
    var b = bSRGB <= 0.04045 ? bSRGB / 12.92 : Math.Pow((bSRGB + 0.055) / 1.055, 2.4);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }

  public static double GetContrastRatio(Color c1, Color c2)
  {
    var relLuminance1 = GetRelativeLuminance(c1);
    var relLuminance2 = GetRelativeLuminance(c2);
    return (Math.Max(relLuminance1, relLuminance2) + 0.05)
        / (Math.Min(relLuminance1, relLuminance2) + 0.05);
  }

  public static Color CompositeAlpha(Color color, Color background)
  {
    double alpha1 = color.A / 255.0;
    double alpha2 = 1.0 - alpha1;

    return new Color()
    {
      A = 255,
      R = (byte)(color.R * alpha1 + background.R * alpha2),
      G = (byte)(color.G * alpha1 + background.G * alpha2),
      B = (byte)(color.B * alpha1 + background.B * alpha2),
    };
  }

  extension(Color color)
  {
    public SolidColorBrush SolidColorBrush => new(color);
    public double RelativeLuminance => GetRelativeLuminance(color);

    public double ContrastRatioTo(Color other) => GetContrastRatio(color, other);

    public Color CompositeAlphaWith(Color background) => CompositeAlpha(color, background);
  }
}
