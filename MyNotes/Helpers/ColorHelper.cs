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

  public static bool IsSimilar(Color c1, Color c2, double threshold = 2.3)
  {
    LabColor lab1 = c1.Lab;
    LabColor lab2 = c2.Lab;

    double deltaE = Math.Sqrt(Math.Pow(lab1.L - lab2.L, 2) + Math.Pow(lab1.A - lab2.A, 2) + Math.Pow(lab1.B - lab2.B, 2));
    return deltaE <= threshold;
  }

  public static LabColor ToLab(Color color)
  {
    // 1. sRGB(0~255) → Linear RGB(0~1) 변환
    double r = SrgbToLinear(color.R / 255.0);
    double g = SrgbToLinear(color.G / 255.0);
    double b = SrgbToLinear(color.B / 255.0);

    // 2. Linear RGB → XYZ 변환
    // 표준 D65 변환 매트릭스 사용
    double x = r * 0.4124 + g * 0.3576 + b * 0.1805;
    double y = r * 0.2126 + g * 0.7152 + b * 0.0722;
    double z = r * 0.0193 + g * 0.1192 + b * 0.9505;

    // 3. XYZ → Lab 변환 (참고 표준 백색점 D65)
    // 백색점
    double Xn = 0.95047, Yn = 1.00000, Zn = 1.08883;

    double fx = Fxyz(x / Xn);
    double fy = Fxyz(y / Yn);
    double fz = Fxyz(z / Zn);

    double l = 116.0 * fy - 16.0;
    double a = 500.0 * (fx - fy);
    double b2 = 200.0 * (fy - fz);

    return new LabColor(l, a, b2);
  }

  private static double SrgbToLinear(double c) => (c > 0.04045) ? Math.Pow((c + 0.055) / 1.055, 2.4) : c / 12.92;

  private static double Fxyz(double t) => (t > Math.Pow(6.0 / 29.0, 3.0)) ? Math.Pow(t, 1.0 / 3.0) : (t / (3 * Math.Pow(6.0 / 29.0, 2.0)) + 4.0 / 29.0);

  public static Color GetComplementary(Color c) => Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

  public static ElementTheme GetThemeFromColor(Color color)
  {
    color = color.CompositeAlphaWith(Colors.White);

    double preferLight = color.ContrastRatioTo(Colors.Black);
    double preferDark = color.ContrastRatioTo(Colors.White);

    return preferLight >= preferDark ? ElementTheme.Light : ElementTheme.Dark;
  }

  extension(Color color)
  {
    public SolidColorBrush SolidColorBrush => new(color);
    public LabColor Lab => ToLab(color);
    public double RelativeLuminance => GetRelativeLuminance(color);
    public Color Complementary => GetComplementary(color);

    public double ContrastRatioTo(Color other) => GetContrastRatio(color, other);

    public Color CompositeAlphaWith(Color background) => CompositeAlpha(color, background);

    public bool IsSimilarTo(Color other, double threshold = 2.3) => IsSimilar(color, other, threshold);
  }
}

internal readonly struct LabColor(double l, double a, double b)
{
  public double L { get; } = l;
  public double A { get; } = a;
  public double B { get; } = b;
}