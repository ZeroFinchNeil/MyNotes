using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyNotes.Templates;

public sealed partial class IconPicker : Control
{
  private static readonly double _iconSize = 32.0;
  private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
  private static Dictionary<short, IconMetadata>? _metadataList;
  private static ILookup<string, IconMetadata>? _metadataGroup;
  private static IconIndex? _iconIndex;
  private static readonly Task _initializeTask;
  private static readonly string? _assemblyName = Assembly.GetAssembly(typeof(IconPicker))?.GetName().Name;
  private static readonly int _recentIconsCount = 24;
  private static readonly LinkedList<IconMetadata> _recentIcons = new();
  private static readonly HashSet<IconMetadata> _recentIconsUniqueSet = new();

  static IconPicker()
  {
    _initializeTask = InitializeAsync();
  }

  private static async Task InitializeAsync()
  {
    using (var stream = _assembly.GetManifestResourceStream($"{_assemblyName}.Resources.Icons.metadata.json"))
    {
      if (stream is not null)
      {
        var jsonResult = await JsonSerializer.DeserializeAsync<IEnumerable<IconMetadata>>(stream);
        _metadataList = jsonResult?.ToDictionary(item => item.Id);
        _metadataGroup = jsonResult?.ToLookup(item => item.Skintone is null ? item.Group : $"{item.Group}.{item.Skintone}");
      }
    }
    using (var stream = _assembly.GetManifestResourceStream($"{_assemblyName}.Resources.Icons.index.json"))
    {
      if (stream is not null)
      {
        _iconIndex = await JsonSerializer.DeserializeAsync<IconIndex>(stream);
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

  public IconPicker()
  {
    DefaultStyleKey = typeof(IconPicker);
  }

  #region Dependency Property
  public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(IconPicker), new PropertyMetadata(null));
  public string? Glyph
  {
    get => (string?)GetValue(GlyphProperty);
    private set => SetValue(GlyphProperty, value);
  }

  public static readonly DependencyProperty IconImageProperty = DependencyProperty.Register("IconImage", typeof(ImageSource), typeof(IconPicker), new PropertyMetadata(null));
  public ImageSource? IconImage
  {
    get => (ImageSource?)GetValue(IconImageProperty);
    private set => SetValue(IconImageProperty, value);
  }

  public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register("DecodePixelWidth", typeof(double), typeof(IconPicker), new PropertyMetadata(0.0, DecodePixel_PropertyChanged));
  public double DecodePixelWidth
  {
    get => (double)GetValue(DecodePixelWidthProperty);
    set => SetValue(DecodePixelWidthProperty, value);
  }

  public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register("DecodePixelHeight", typeof(double), typeof(IconPicker), new PropertyMetadata(0.0, DecodePixel_PropertyChanged));
  public double DecodePixelHeight
  {
    get => (double)GetValue(DecodePixelHeightProperty);
    set => SetValue(DecodePixelHeightProperty, value);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Icon?), typeof(IconPicker), new PropertyMetadata(null, Icon_PropertyChanged));
  public Icon? Icon
  {
    get => (Icon?)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }
  #endregion

  private async static void DecodePixel_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is IconPicker control)
    {
      await _initializeTask;
      if (_metadataList is not null && control.Icon is Icon icon && _metadataList.TryGetValue((short)icon, out var metadata))
      {
        var scale = control.XamlRoot?.RasterizationScale ?? control.RasterizationScale;
        control.IconImage = await GetBitmapImage(metadata, (int)(control.DecodePixelWidth * scale), (int)(control.DecodePixelHeight * scale));
      }
    }
  }

  private async static void Icon_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is IconPicker control)
    {
      await _initializeTask;
      if (e.NewValue is Icon icon && _metadataList is not null)
      {
        if (_metadataList.TryGetValue((short)icon, out var metadata))
        {
          var scale = control.XamlRoot?.RasterizationScale ?? control.RasterizationScale;
          control.Glyph = metadata.Unicode16;
          control.IconImage = await GetBitmapImage(metadata, (int)(control.DecodePixelWidth * scale), (int)(control.DecodePixelHeight * scale));
        }
      }
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
          button.Click += async (s, e) =>
          {
            this.Icon = (Icon)iconMetadata.Id;
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
        button.Click += async (s, e) =>
        {
          this.Icon = (Icon)metadata.Id;
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
            button.Click += async (s, e) =>
            {
              this.Icon = (Icon)metadata.Id;
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

    if (await GetBitmapImage(metadata, (int)size, (int)size) is BitmapImage bitmapImage)
    {
      Button button = new()
      {
        DataContext = metadata,
        Content = new Image() { Source = bitmapImage, Width = size, Height = size },
        Style = _buttonStyle
      };
      ToolTipService.SetToolTip(button, metadata.Description);

      return button;
    }

    return null;
  }

  private static async Task<BitmapImage?> GetBitmapImage(IconMetadata metadata, int width = 0, int height = 0)
  {
    using var stream = _assembly.GetManifestResourceStream($"{_assemblyName}.Resources.Icons.Images.{metadata.Id}");
    if (stream is not null)
    {
      BitmapImage bitmapImage = width > 0 && height > 0
        ? new() { DecodePixelWidth = width, DecodePixelHeight = height }
        : new();

      using (MemoryStream memoryStream = new())
      {
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
      }
      return bitmapImage;
    }
    return null;
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
  private SelectorBarItem? SimpleIconsSelectorBarItem;

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
    SimpleIconsSelectorBarItem = GetTemplateChild("SimpleIconsSelectorBarItem") as SelectorBarItem;

    ToolTipService.SetToolTip(RecentSelectorBarItem, "Recent & Search");
    ToolTipService.SetToolTip(ObjectsAndActivitiesSelectorBarItem, "Objects & Activities");
    ToolTipService.SetToolTip(AnimalsAndNatureSelectorBarItem, "Animals & Nature");
    ToolTipService.SetToolTip(FoodAndDrinkSelectorBarItem, "Food & Drink");
    ToolTipService.SetToolTip(PeopleAndBodySelectorBarItem, "People & Body");
    ToolTipService.SetToolTip(SmileysAndEmotionSelectorBarItem, "Smileys & Emotion");
    ToolTipService.SetToolTip(TravelAndPlacesSelectorBarItem, "Travel & Places");
    ToolTipService.SetToolTip(SymbolsAndFlagsSelectorBarItem, "Symbols & Flags");
    ToolTipService.SetToolTip(SimpleIconsSelectorBarItem, "Simple Icons");

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
      SelectorBarItem item when item == SimpleIconsSelectorBarItem => [.. await GetIconButton(IconGroup.Color, scale)],
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