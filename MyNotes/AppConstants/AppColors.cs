using CommunityToolkit.WinUI.Helpers;

namespace MyNotes.AppConstants;

internal static class AppColors
{
  public static readonly IReadOnlyList<Color> NamedPaletteColors = new List<Color>()
  {
    "#fffafa".ToColor(), "#fff5ee".ToColor(), "#fdf5e6".ToColor(), "#fffaf0".ToColor(), "#ffffe0".ToColor(),
    "#fffff0".ToColor(), "#f5fffa".ToColor(), "#e0ffff".ToColor(), "#f0ffff".ToColor(), "#f0f8ff".ToColor(),
    "#f8f8ff".ToColor(), "#d8bfd8".ToColor(), "#dda0dd".ToColor(), "#fff0f5".ToColor(), "#ffffff".ToColor(),
    "#ffe4e1".ToColor(), "#faf0e6".ToColor(), "#faebd7".ToColor(), "#ffefd5".ToColor(), "#fff8dc".ToColor(),
    "#f0fff0".ToColor(), "#00fa9a".ToColor(), "#afeeee".ToColor(), "#b0e0e6".ToColor(), "#b0c4de".ToColor(),
    "#e6e6fa".ToColor(), "#ba55d3".ToColor(), "#ee82ee".ToColor(), "#ffc0cb".ToColor(), "#f5f5f5".ToColor(),
    "#f08080".ToColor(), "#ffa07a".ToColor(), "#ffe4c4".ToColor(), "#ffebcd".ToColor(), "#f5f5dc".ToColor(),
    "#90ee90".ToColor(), "#00ff7f".ToColor(), "#7fffd4".ToColor(), "#add8e6".ToColor(), "#87cefa".ToColor(),
    "#0000ff".ToColor(), "#9932cc".ToColor(), "#da70d6".ToColor(), "#ffb6c1".ToColor(), "#dcdcdc".ToColor(),
    "#cd5c5c".ToColor(), "#e9967a".ToColor(), "#ffdab9".ToColor(), "#ffe4b5".ToColor(), "#fafad2".ToColor(),
    "#98fb98".ToColor(), "#00ff00".ToColor(), "#00ffff".ToColor(), "#00ced1".ToColor(), "#87ceeb".ToColor(), 
    "#0000cd".ToColor(), "#8a2be2".ToColor(), "#ff00ff".ToColor(), "#db7093".ToColor(), "#d3d3d3".ToColor(), 
    "#bc8f8f".ToColor(), "#ff7f50".ToColor(), "#deb887".ToColor(), "#f5deb3".ToColor(), "#fffacd".ToColor(),
    "#7fff00".ToColor(), "#32cd32".ToColor(), "#40e0d0".ToColor(), "#20b2aa".ToColor(), "#00bfff".ToColor(), 
    "#00008b".ToColor(), "#9370db".ToColor(), "#8b008b".ToColor(), "#ff69b4".ToColor(), "#c0c0c0".ToColor(),
    "#ff0000".ToColor(), "#ff4500".ToColor(), "#d2b48c".ToColor(), "#ffdead".ToColor(), "#ffff00".ToColor(),
    "#7cfc00".ToColor(), "#8fbc8f".ToColor(), "#48d1cc".ToColor(), "#5f9ea0".ToColor(), "#4169e1".ToColor(),
    "#000080".ToColor(), "#7b68ee".ToColor(), "#800080".ToColor(), "#ff1493".ToColor(), "#a9a9a9".ToColor(),
    "#b22222".ToColor(), "#ff6347".ToColor(), "#f4a460".ToColor(), "#ffd700".ToColor(), "#f0e68c".ToColor(),
    "#adff2f".ToColor(), "#228b22".ToColor(), "#66cdaa".ToColor(), "#008b8b".ToColor(), "#6495ed".ToColor(),
    "#191970".ToColor(), "#6a5acd".ToColor(), "#9400d3".ToColor(), "#c71585".ToColor(), "#808080".ToColor(), 
    "#8b0000".ToColor(), "#fa8072".ToColor(), "#ff8c00".ToColor(), "#ffa500".ToColor(), "#eee8aa".ToColor(), 
    "#9acd32".ToColor(), "#008000".ToColor(), "#3cb371".ToColor(), "#008080".ToColor(), "#1e90ff".ToColor(),
    "#778899".ToColor(), "#483d8b".ToColor(), "#4b0082".ToColor(), "#dc143c".ToColor(), "#696969".ToColor(),
    "#800000".ToColor(), "#a0522d".ToColor(), "#d2691e".ToColor(), "#daa520".ToColor(), "#bdb76b".ToColor(),
    "#6b8e23".ToColor(), "#006400".ToColor(), "#2e8b57".ToColor(), "#2f4f4f".ToColor(), "#4682b4".ToColor(),
    "#708090".ToColor(), "#663399".ToColor(), "#000000".ToColor(), "#a52a2a".ToColor(), "#8b4513".ToColor(),
    "#cd853f".ToColor(), "#b8860b".ToColor(), "#808000".ToColor(), "#556b2f".ToColor(),
  }.AsReadOnly();

  public static readonly IReadOnlyList<SolidColorBrush> NamedPaletteColorBrushes = new List<SolidColorBrush>()
  {
    new("#fffafa".ToColor()), new("#fff5ee".ToColor()), new("#fdf5e6".ToColor()), new("#fffaf0".ToColor()), new("#ffffe0".ToColor()),
    new("#fffff0".ToColor()), new("#f5fffa".ToColor()), new("#e0ffff".ToColor()), new("#f0ffff".ToColor()), new("#f0f8ff".ToColor()),
    new("#f8f8ff".ToColor()), new("#d8bfd8".ToColor()), new("#dda0dd".ToColor()), new("#fff0f5".ToColor()), new("#ffffff".ToColor()),
    new("#ffe4e1".ToColor()), new("#faf0e6".ToColor()), new("#faebd7".ToColor()), new("#ffefd5".ToColor()), new("#fff8dc".ToColor()),
    new("#f0fff0".ToColor()), new("#00fa9a".ToColor()), new("#afeeee".ToColor()), new("#b0e0e6".ToColor()), new("#b0c4de".ToColor()),
    new("#e6e6fa".ToColor()), new("#ba55d3".ToColor()), new("#ee82ee".ToColor()), new("#ffc0cb".ToColor()), new("#f5f5f5".ToColor()),
    new("#f08080".ToColor()), new("#ffa07a".ToColor()), new("#ffe4c4".ToColor()), new("#ffebcd".ToColor()), new("#f5f5dc".ToColor()),
    new("#90ee90".ToColor()), new("#00ff7f".ToColor()), new("#7fffd4".ToColor()), new("#add8e6".ToColor()), new("#87cefa".ToColor()),
    new("#0000ff".ToColor()), new("#9932cc".ToColor()), new("#da70d6".ToColor()), new("#ffb6c1".ToColor()), new("#dcdcdc".ToColor()),
    new("#cd5c5c".ToColor()), new("#e9967a".ToColor()), new("#ffdab9".ToColor()), new("#ffe4b5".ToColor()), new("#fafad2".ToColor()),
    new("#98fb98".ToColor()), new("#00ff00".ToColor()), new("#00ffff".ToColor()), new("#00ced1".ToColor()), new("#87ceeb".ToColor()),
    new("#0000cd".ToColor()), new("#8a2be2".ToColor()), new("#ff00ff".ToColor()), new("#db7093".ToColor()), new("#d3d3d3".ToColor()),
    new("#bc8f8f".ToColor()), new("#ff7f50".ToColor()), new("#deb887".ToColor()), new("#f5deb3".ToColor()), new("#fffacd".ToColor()),
    new("#7fff00".ToColor()), new("#32cd32".ToColor()), new("#40e0d0".ToColor()), new("#20b2aa".ToColor()), new("#00bfff".ToColor()),
    new("#00008b".ToColor()), new("#9370db".ToColor()), new("#8b008b".ToColor()), new("#ff69b4".ToColor()), new("#c0c0c0".ToColor()),
    new("#ff0000".ToColor()), new("#ff4500".ToColor()), new("#d2b48c".ToColor()), new("#ffdead".ToColor()), new("#ffff00".ToColor()),
    new("#7cfc00".ToColor()), new("#8fbc8f".ToColor()), new("#48d1cc".ToColor()), new("#5f9ea0".ToColor()), new("#4169e1".ToColor()),
    new("#000080".ToColor()), new("#7b68ee".ToColor()), new("#800080".ToColor()), new("#ff1493".ToColor()), new("#a9a9a9".ToColor()),
    new("#b22222".ToColor()), new("#ff6347".ToColor()), new("#f4a460".ToColor()), new("#ffd700".ToColor()), new("#f0e68c".ToColor()),
    new("#adff2f".ToColor()), new("#228b22".ToColor()), new("#66cdaa".ToColor()), new("#008b8b".ToColor()), new("#6495ed".ToColor()),
    new("#191970".ToColor()), new("#6a5acd".ToColor()), new("#9400d3".ToColor()), new("#c71585".ToColor()), new("#808080".ToColor()),
    new("#8b0000".ToColor()), new("#fa8072".ToColor()), new("#ff8c00".ToColor()), new("#ffa500".ToColor()), new("#eee8aa".ToColor()),
    new("#9acd32".ToColor()), new("#008000".ToColor()), new("#3cb371".ToColor()), new("#008080".ToColor()), new("#1e90ff".ToColor()),
    new("#778899".ToColor()), new("#483d8b".ToColor()), new("#4b0082".ToColor()), new("#dc143c".ToColor()), new("#696969".ToColor()),
    new("#800000".ToColor()), new("#a0522d".ToColor()), new("#d2691e".ToColor()), new("#daa520".ToColor()), new("#bdb76b".ToColor()),
    new("#6b8e23".ToColor()), new("#006400".ToColor()), new("#2e8b57".ToColor()), new("#2f4f4f".ToColor()), new("#4682b4".ToColor()),
    new("#708090".ToColor()), new("#663399".ToColor()), new("#000000".ToColor()), new("#a52a2a".ToColor()), new("#8b4513".ToColor()),
    new("#cd853f".ToColor()), new("#b8860b".ToColor()), new("#808000".ToColor()), new("#556b2f".ToColor()),
  }.AsReadOnly();

  public static readonly IReadOnlyList<Color> DefaultPaletteColors = new List<Color>()
  {
    "#FFCCCC".ToColor(), "#F59F9F".ToColor(), "#EB7575".ToColor(), "#E04F4F".ToColor(), "#D62B2B".ToColor(),
    "#FFE6CC".ToColor(), "#F5CA9F".ToColor(), "#EBB075".ToColor(), "#E0974F".ToColor(), "#D6812B".ToColor(),
    "#FFF2CC".ToColor(), "#F5DF9F".ToColor(), "#EBCD75".ToColor(), "#E0BC4F".ToColor(), "#D6AB2B".ToColor(),
    "#FFFFCC".ToColor(), "#F2F291".ToColor(), "#E6E65C".ToColor(), "#D9D92B".ToColor(), "#CCCC00".ToColor(),
    "#E6FFCC".ToColor(), "#CAF59F".ToColor(), "#B0EB75".ToColor(), "#97E04F".ToColor(), "#81D62B".ToColor(),
    "#CCFFCC".ToColor(), "#8AE68A".ToColor(), "#52CC52".ToColor(), "#24B224".ToColor(), "#009900".ToColor(),
    "#CCFFE6".ToColor(), "#91F2C2".ToColor(), "#5CE6A1".ToColor(), "#2BD982".ToColor(), "#00CC66".ToColor(),
    "#CCFFFF".ToColor(), "#91F2F2".ToColor(), "#5CE6E6".ToColor(), "#2BD9D9".ToColor(), "#00CCCC".ToColor(),
    "#CCE6FF".ToColor(), "#9FCAF5".ToColor(), "#75B0EB".ToColor(), "#4F97E0".ToColor(), "#2B81D6".ToColor(),
    "#CCCCFF".ToColor(), "#9F9FF5".ToColor(), "#7575EB".ToColor(), "#4F4FE0".ToColor(), "#2B2BD6".ToColor(),
    "#E6CCFF".ToColor(), "#CA9FF5".ToColor(), "#B075EB".ToColor(), "#974FE0".ToColor(), "#812BD6".ToColor(),
    "#FFCCFF".ToColor(), "#F59FF5".ToColor(), "#EB75EB".ToColor(), "#E04FE0".ToColor(), "#D62BD6".ToColor(),
    "#FFCCE6".ToColor(), "#F59FCA".ToColor(), "#EB75B0".ToColor(), "#E04F97".ToColor(), "#D62B81".ToColor(),
    "#F2F2F2".ToColor(), "#D9D9D9".ToColor(), "#BFBFBF".ToColor(), "#A6A6A6".ToColor(), "#8C8C8C".ToColor(),
    "#737373".ToColor(), "#595959".ToColor(), "#404040".ToColor(), "#262626".ToColor(), "#0D0D0D".ToColor(),
  }.AsReadOnly();

  public static readonly IReadOnlyList<SolidColorBrush> DefaultPaletteColorBrushes = new List<SolidColorBrush>()
  {
    new("#FFCCCC".ToColor()), new("#F59F9F".ToColor()), new("#EB7575".ToColor()), new("#E04F4F".ToColor()), new("#D62B2B".ToColor()),
    new("#FFE6CC".ToColor()), new("#F5CA9F".ToColor()), new("#EBB075".ToColor()), new("#E0974F".ToColor()), new("#D6812B".ToColor()),
    new("#FFF2CC".ToColor()), new("#F5DF9F".ToColor()), new("#EBCD75".ToColor()), new("#E0BC4F".ToColor()), new("#D6AB2B".ToColor()),
    new("#FFFFCC".ToColor()), new("#F2F291".ToColor()), new("#E6E65C".ToColor()), new("#D9D92B".ToColor()), new("#CCCC00".ToColor()),
    new("#E6FFCC".ToColor()), new("#CAF59F".ToColor()), new("#B0EB75".ToColor()), new("#97E04F".ToColor()), new("#81D62B".ToColor()),
    new("#CCFFCC".ToColor()), new("#8AE68A".ToColor()), new("#52CC52".ToColor()), new("#24B224".ToColor()), new("#009900".ToColor()),
    new("#CCFFE6".ToColor()), new("#91F2C2".ToColor()), new("#5CE6A1".ToColor()), new("#2BD982".ToColor()), new("#00CC66".ToColor()),
    new("#CCFFFF".ToColor()), new("#91F2F2".ToColor()), new("#5CE6E6".ToColor()), new("#2BD9D9".ToColor()), new("#00CCCC".ToColor()),
    new("#CCE6FF".ToColor()), new("#9FCAF5".ToColor()), new("#75B0EB".ToColor()), new("#4F97E0".ToColor()), new("#2B81D6".ToColor()),
    new("#CCCCFF".ToColor()), new("#9F9FF5".ToColor()), new("#7575EB".ToColor()), new("#4F4FE0".ToColor()), new("#2B2BD6".ToColor()),
    new("#E6CCFF".ToColor()), new("#CA9FF5".ToColor()), new("#B075EB".ToColor()), new("#974FE0".ToColor()), new("#812BD6".ToColor()),
    new("#FFCCFF".ToColor()), new("#F59FF5".ToColor()), new("#EB75EB".ToColor()), new("#E04FE0".ToColor()), new("#D62BD6".ToColor()),
    new("#FFCCE6".ToColor()), new("#F59FCA".ToColor()), new("#EB75B0".ToColor()), new("#E04F97".ToColor()), new("#D62B81".ToColor()),
    new("#F2F2F2".ToColor()), new("#D9D9D9".ToColor()), new("#BFBFBF".ToColor()), new("#A6A6A6".ToColor()), new("#8C8C8C".ToColor()),
    new("#737373".ToColor()), new("#595959".ToColor()), new("#404040".ToColor()), new("#262626".ToColor()), new("#0D0D0D".ToColor()),
  }.AsReadOnly();
}
