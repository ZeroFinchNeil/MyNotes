using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyNotes.Templates;

public sealed partial class IconPicker : Control
{
  private static readonly double _iconSize = 32.0;
  private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
  private static readonly Dictionary<short, IconMetadata>? _metadataList;
  private static readonly ILookup<string, IconMetadata>? _metadataGroup;
  private static readonly IconIndex? _iconIndex;
  private static readonly string? _assemblyName = Assembly.GetAssembly(typeof(IconPicker))?.GetName().Name;
  private static readonly int _recentIconsCount = 24;
  private static readonly LinkedList<IconMetadata> _recentIcons = new();
  private static readonly HashSet<IconMetadata> _recentIconsUniqueSet = new();

  static IconPicker()
  {
    using (var stream = _assembly.GetManifestResourceStream($"{_assemblyName}.Resources.Icons.metadata.json"))
    {
      if (stream is not null)
      {
        var jsonResult = JsonSerializer.Deserialize<IEnumerable<IconMetadata>>(stream);
        _metadataList = jsonResult?.ToDictionary(item => item.Id);
        _metadataGroup = jsonResult?.ToLookup(item => item.Skintone is null ? item.Group : $"{item.Group}.{item.Skintone}");
      }
    }

    using (var stream = _assembly.GetManifestResourceStream($"{_assemblyName}.Resources.Icons.index.json"))
    {
      if (stream is not null)
      {
        _iconIndex = JsonSerializer.Deserialize<IconIndex>(stream);
      }
    }
  }

  private static void AddRecentIcon(IconMetadata metadata)
  {
    if (_recentIconsUniqueSet.Contains(metadata))
    {
      _recentIcons.Remove(metadata);
      _recentIcons.AddFirst(metadata);
    }
    else
    {
      while (_recentIcons.Count >= _recentIconsCount)
      {
        _recentIconsUniqueSet.Remove(_recentIcons.Last());
        _recentIcons.RemoveLast();
      }
      _recentIconsUniqueSet.Add(metadata);
      _recentIcons.AddFirst(metadata);
    }
  }

  private async Task<IList<Button>> GetIconButton(string groupKey, double scale = 1.0)
  {
    List<Button> buttons = new();
    if (_metadataGroup is not null)
    {
      foreach (var iconMetadata in _metadataGroup[groupKey])
      {
        if (await CreateButton(iconMetadata, scale) is Button button)
        {
          button.Click += (s, e) =>
          {
            this.Glyph = iconMetadata.Unicode16;
            this.Unicode32Seqeunce = iconMetadata.Unicode32Sequence;
            AddRecentIcon(iconMetadata);
          };
          buttons.Add(button);
        }
      }
    }
    return buttons;
  }

  private async Task<IList<Button>> GetHistoryButton(double scale = 1.0)
  {
    List<Button> buttons = new();

    foreach (var metadata in _recentIcons)
    {
      if (await CreateButton(metadata, scale) is Button button)
      {
        button.Click += (s, e) =>
        {
          this.Glyph = metadata.Unicode16;
          this.Unicode32Seqeunce = metadata.Unicode32Sequence;
        };
        buttons.Add(button);
      }
    }

    return buttons;
  }

  private async Task<IList<Button>> SearchIconButton(string word, double scale = 1.0)
  {
    List<Button> buttons = new();

    if (_metadataList is not null && _iconIndex is not null)
    {
      if (_iconIndex.Terms.TryGetValue(word, out var ids))
      {
        foreach (var id in ids)
        {
          if (_metadataList.TryGetValue(id, out var metadata) && await CreateButton(metadata, scale) is Button button)
          {
            button.Click += (s, e) =>
            {
              this.Glyph = metadata.Unicode16;
              this.Unicode32Seqeunce = metadata.Unicode32Sequence;
              AddRecentIcon(metadata);
            };
            buttons.Add(button);
          }
        }
      }
    }
    return buttons;
  }

  private async Task<Button?> CreateButton(IconMetadata metadata, double scale)
  {
    double size = _iconSize * scale;
    using var stream = _assembly.GetManifestResourceStream($"{_assemblyName}.Resources.Icons.FluentEmoji.{metadata.Unicode32Sequence}");
    if (stream is not null)
    {
      BitmapImage bitmapImage = new() { DecodePixelHeight = (int)size, DecodePixelWidth = (int)size };
      using (MemoryStream memoryStream = new())
      {
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
      }

      Button button = new()
      {
        DataContext = metadata,
        Content = new Image() { Source = bitmapImage, Width = size, Height = size },
        Style = _buttonStyle
      };
      ToolTipService.SetToolTip(button, metadata.Name);

      return button;
    }

    return null;
  }

  public IconPicker()
  {
    DefaultStyleKey = typeof(IconPicker);
  }

  public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(IconPicker), new PropertyMetadata(null));
  public string Glyph
  {
    get => (string)GetValue(GlyphProperty);
    set => SetValue(GlyphProperty, value);
  }

  public static readonly DependencyProperty Unicode32SeqeunceProperty = DependencyProperty.Register("Unicode32Seqeunce", typeof(string), typeof(IconPicker), new PropertyMetadata(null));
  public string Unicode32Seqeunce
  {
    get => (string)GetValue(Unicode32SeqeunceProperty);
    set => SetValue(Unicode32SeqeunceProperty, value);
  }

  private Style? _buttonStyle;
  private Grid? RootGrid;

  private SelectorBar? HeaderSelectorBar;
  private SelectorBarItem? RecentSelectorBarItem;
  private SelectorBarItem? ObjectsAndActivitiesSelectorBarItem;
  private SelectorBarItem? AnimalsAndNatureSelectorBarItem;
  private SelectorBarItem? FoodAndDrinkSelectorBarItem;
  private SelectorBarItem? PeopleAndBodySelectorBarItem;
  private SelectorBarItem? SmileysAndEmotionSelectorBarItem;
  private SelectorBarItem? TravelAndPlacesSelectorBarItem;
  private SelectorBarItem? SymbolsAndFlagsSelectorBarItem;

  private ScrollView? IconsScrollView;
  private ItemsRepeater? IconsItemsRepeater;

  private AutoSuggestBox? SearchAutoSuggestBox;
  private ItemsRepeater? SearchItemsRepeater;

  private RadioButtons? SkintoneRadioButtons;
  private RadioButton? SkintoneDefaultButton;
  private RadioButton? SkintoneLightButton;
  private RadioButton? SkintoneMediumLightButton;
  private RadioButton? SkintoneMediumButton;
  private RadioButton? SkintoneMediumDarkButton;
  private RadioButton? SkintoneDarkButton;

  protected override async void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    RootGrid = GetTemplateChild("RootGrid") as Grid;
    if (RootGrid is not null
        && RootGrid.Resources.TryGetValue("IconButtonStyle", out var resource)
        && resource is Style buttonStyle)
    {
      _buttonStyle = buttonStyle;
    }

    HeaderSelectorBar = GetTemplateChild("HeaderSelectorBar") as SelectorBar;
    HeaderSelectorBar?.SelectionChanged += HeaderSelectorBar_SelectionChanged;

    RecentSelectorBarItem = GetTemplateChild("RecentSelectorBarItem") as SelectorBarItem;
    ObjectsAndActivitiesSelectorBarItem = GetTemplateChild("ObjectsAndActivitiesSelectorBarItem") as SelectorBarItem;
    AnimalsAndNatureSelectorBarItem = GetTemplateChild("AnimalsAndNatureSelectorBarItem") as SelectorBarItem;
    FoodAndDrinkSelectorBarItem = GetTemplateChild("FoodAndDrinkSelectorBarItem") as SelectorBarItem;
    PeopleAndBodySelectorBarItem = GetTemplateChild("PeopleAndBodySelectorBarItem") as SelectorBarItem;
    SmileysAndEmotionSelectorBarItem = GetTemplateChild("SmileysAndEmotionSelectorBarItem") as SelectorBarItem;
    TravelAndPlacesSelectorBarItem = GetTemplateChild("TravelAndPlacesSelectorBarItem") as SelectorBarItem;
    SymbolsAndFlagsSelectorBarItem = GetTemplateChild("SymbolsAndFlagsSelectorBarItem") as SelectorBarItem;

    ToolTipService.SetToolTip(RecentSelectorBarItem, "Recent history");
    ToolTipService.SetToolTip(ObjectsAndActivitiesSelectorBarItem, "Objects & Activities");
    ToolTipService.SetToolTip(AnimalsAndNatureSelectorBarItem, "Animals & Nature");
    ToolTipService.SetToolTip(FoodAndDrinkSelectorBarItem, "Food & Drink");
    ToolTipService.SetToolTip(PeopleAndBodySelectorBarItem, "People & Body");
    ToolTipService.SetToolTip(SmileysAndEmotionSelectorBarItem, "Smileys & Emotion");
    ToolTipService.SetToolTip(TravelAndPlacesSelectorBarItem, "Travel & Places");
    ToolTipService.SetToolTip(SymbolsAndFlagsSelectorBarItem, "Symbols & Flags");

    IconsScrollView = GetTemplateChild("IconsScrollView") as ScrollView;
    IconsItemsRepeater = GetTemplateChild("IconsItemsRepeater") as ItemsRepeater;

    SearchAutoSuggestBox = GetTemplateChild("SearchAutoSuggestBox") as AutoSuggestBox;
    SearchItemsRepeater = GetTemplateChild("SearchItemsRepeater") as ItemsRepeater;

    SearchAutoSuggestBox?.TextChanged += async (s, e) =>
    {
      if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
      {
        string word = s.Text.Trim().ToLower();
        SearchItemsRepeater?.ItemsSource = string.IsNullOrEmpty(word) ? null : await SearchIconButton(word, this.XamlRoot.RasterizationScale);
      }
    };

    SkintoneRadioButtons = GetTemplateChild("SkintoneRadioButtons") as RadioButtons;
    SkintoneDefaultButton = GetTemplateChild("SkintoneDefaultButton") as RadioButton;
    SkintoneLightButton = GetTemplateChild("SkintoneLightButton") as RadioButton;
    SkintoneMediumLightButton = GetTemplateChild("SkintoneMediumLightButton") as RadioButton;
    SkintoneMediumButton = GetTemplateChild("SkintoneMediumButton") as RadioButton;
    SkintoneMediumDarkButton = GetTemplateChild("SkintoneMediumDarkButton") as RadioButton;
    SkintoneDarkButton = GetTemplateChild("SkintoneDarkButton") as RadioButton;

    SkintoneRadioButtons?.SelectionChanged += SkintoneRadioButtons_SelectionChanged;
    _selectedSkintoneButton = SkintoneDefaultButton;

    HeaderSelectorBar?.SelectedItem = _recentIcons.Count > 0 ? RecentSelectorBarItem : ObjectsAndActivitiesSelectorBarItem;
  }

  private async void HeaderSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    var scale = this.XamlRoot.RasterizationScale;
    IconsItemsRepeater?.ItemsSource = new List<Button>(sender.SelectedItem switch
    {
      SelectorBarItem item when item == RecentSelectorBarItem => [.. await GetHistoryButton(scale)],
      SelectorBarItem item when item == ObjectsAndActivitiesSelectorBarItem => [.. await GetIconButton(IconGroup.Objects, scale), .. await GetIconButton(IconGroup.Activities, scale)],
      SelectorBarItem item when item == AnimalsAndNatureSelectorBarItem => await GetIconButton(IconGroup.AnimalsAndNature, scale),
      SelectorBarItem item when item == FoodAndDrinkSelectorBarItem => await GetIconButton(IconGroup.FoodAndDrink, scale),
      SelectorBarItem item when item == SmileysAndEmotionSelectorBarItem => await GetIconButton(IconGroup.SmileysAndEmotion, scale),
      SelectorBarItem item when item == TravelAndPlacesSelectorBarItem => await GetIconButton(IconGroup.TravelAndPlaces, scale),
      SelectorBarItem item when item == SymbolsAndFlagsSelectorBarItem => [.. await GetIconButton(IconGroup.Symbols, scale), .. await GetIconButton(IconGroup.Flags, scale)],
      _ => [],
    });

    if (sender.SelectedItem == PeopleAndBodySelectorBarItem)
    {
      VisualStateManager.GoToState(this, "SkintonePanelVisible", false);
      SkintoneRadioButtons?.SelectedItem = _selectedSkintoneButton;
    }
    else
    {
      VisualStateManager.GoToState(this, "SkintonePanelCollapsed", false);
      SkintoneRadioButtons?.SelectedItem = null;
    }

    if (sender.SelectedItem == RecentSelectorBarItem)
    {
      VisualStateManager.GoToState(this, "SearchPanelVisible", false);
    }
    else
    {
      VisualStateManager.GoToState(this, "SearchPanelCollapsed", false);
    }

    IconsScrollView?.ScrollTo(0.0, 0.0);
  }

  private RadioButton? _selectedSkintoneButton;
  private async void SkintoneRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (SkintoneRadioButtons?.SelectedItem is RadioButton selected)
    {
      var scale = this.XamlRoot.RasterizationScale;
      _selectedSkintoneButton = selected;
      IconsItemsRepeater?.ItemsSource = new List<Button>(selected switch
      {
        RadioButton button when button == SkintoneDefaultButton => [.. await GetIconButton(IconGroup.PeopleAndBody, scale), .. await GetIconButton(IconGroup.PeopleAndBodyDefault, scale)],
        RadioButton button when button == SkintoneLightButton => [.. await GetIconButton(IconGroup.PeopleAndBody, scale), .. await GetIconButton(IconGroup.PeopleAndBodyLight, scale)],
        RadioButton button when button == SkintoneMediumLightButton => [.. await GetIconButton(IconGroup.PeopleAndBody, scale), .. await GetIconButton(IconGroup.PeopleAndBodyMediumLight, scale)],
        RadioButton button when button == SkintoneMediumButton => [.. await GetIconButton(IconGroup.PeopleAndBody, scale), .. await GetIconButton(IconGroup.PeopleAndBodyMedium, scale)],
        RadioButton button when button == SkintoneMediumDarkButton => [.. await GetIconButton(IconGroup.PeopleAndBody, scale), .. await GetIconButton(IconGroup.PeopleAndBodyMediumDark, scale)],
        RadioButton button when button == SkintoneDarkButton => [.. await GetIconButton(IconGroup.PeopleAndBody, scale), .. await GetIconButton(IconGroup.PeopleAndBodyDark, scale)],
        _ => []
      });
    }
  }
}

public record IconMetadata : IComparable<IconMetadata>
{
  public required short Id { get; set; }
  public required string Group { get; set; }
  public string? Skintone { get; set; }
  public required string Name { get; set; }
  public required string Unicode16 { get; set; }
  public required int[] Unicode32CodePoints { get; set; }
  public required string Unicode32Sequence { get; set; }
  public required string[] Keywords { get; set; }

  public int CompareTo(IconMetadata? other)
  {
    if (other is null)
      return 1;

    int first = (Group + Skintone).CompareTo(other.Group + other.Skintone);
    return first == 0 ? Unicode32Sequence.CompareTo(other.Unicode32Sequence) : first;
  }
}

public static class IconGroup
{
  public static readonly string Objects = "Objects";
  public static readonly string Activities = "Activities";
  public static readonly string AnimalsAndNature = "Animals & Nature";
  public static readonly string FoodAndDrink = "Food & Drink";
  public static readonly string PeopleAndBody = "People & Body";
  public static readonly string SmileysAndEmotion = "Smileys & Emotion";
  public static readonly string TravelAndPlaces = "Travel & Places";
  public static readonly string Symbols = "Symbols";
  public static readonly string Flags = "Flags";

  public static readonly string PeopleAndBodyDefault = "People & Body.default";
  public static readonly string PeopleAndBodyLight = "People & Body.light";
  public static readonly string PeopleAndBodyMediumLight = "People & Body.medium-light";
  public static readonly string PeopleAndBodyMedium = "People & Body.medium";
  public static readonly string PeopleAndBodyMediumDark = "People & Body.medium-dark";
  public static readonly string PeopleAndBodyDark = "People & Body.dark";
}

public record IconIndex
{
  public required Dictionary<string, List<short>> Terms { get; set; }
}