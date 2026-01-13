using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

using Windows.UI;

namespace MyNotes.Templates;

public sealed partial class FindReplaceBox : Control
{
  public FindReplaceBox()
  {
    DefaultStyleKey = typeof(FindReplaceBox);
  }

  public static readonly DependencyProperty TargetEditorProperty = DependencyProperty.Register("Target", typeof(RichEditBox), typeof(FindReplaceBox), new PropertyMetadata(null, OnTargetChanged));
  public RichEditBox TargetEditor
  {
    get => (RichEditBox)GetValue(TargetEditorProperty);
    set => SetValue(TargetEditorProperty, value);
  }

  public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(FindReplaceBox), new PropertyMetadata(false, OnIsOpenChanged));
  public bool IsOpen
  {
    get => (bool)GetValue(IsOpenProperty);
    set => SetValue(IsOpenProperty, value);
  }

  public static readonly DependencyProperty FindHeaderProperty = DependencyProperty.Register("FindHeader", typeof(string), typeof(FindReplaceBox), new PropertyMetadata(string.Empty));
  public string FindHeader
  {
    get => (string)GetValue(FindHeaderProperty);
    set => SetValue(FindHeaderProperty, value);
  }

  public static readonly DependencyProperty ReplaceHeaderProperty = DependencyProperty.Register("ReplaceHeader", typeof(string), typeof(FindReplaceBox), new PropertyMetadata(string.Empty));
  public string ReplaceHeader
  {
    get => (string)GetValue(ReplaceHeaderProperty);
    set => SetValue(ReplaceHeaderProperty, value);
  }

  public static readonly DependencyProperty FindReplaceModeProperty = DependencyProperty.Register("FindReplaceMode", typeof(FindReplaceMode), typeof(FindReplaceBox), new PropertyMetadata(FindReplaceMode.Find, OnFindReplaceModeChanged));
  public FindReplaceMode FindReplaceMode
  {
    get => (FindReplaceMode)GetValue(FindReplaceModeProperty);
    set => SetValue(FindReplaceModeProperty, value);
  }

  public static readonly DependencyProperty FindTextProperty = DependencyProperty.Register("FindText", typeof(string), typeof(FindReplaceBox), new PropertyMetadata(string.Empty, OnFindTextChanged));
  public string FindText
  {
    get => (string)GetValue(FindTextProperty);
    private set => SetValue(FindTextProperty, value);
  }

  public static readonly DependencyProperty ReplaceTextProperty = DependencyProperty.Register("ReplaceText", typeof(string), typeof(FindReplaceBox), new PropertyMetadata(string.Empty, OnReplaceTextChanged));
  public string ReplaceText
  {
    get => (string)GetValue(ReplaceTextProperty);
    private set => SetValue(ReplaceTextProperty, value);
  }

  public static readonly DependencyProperty MatchColorProperty = DependencyProperty.Register("MatchColor", typeof(Color), typeof(FindReplaceBox), new PropertyMetadata(Colors.Yellow));
  public Color MatchColor
  {
    get => (Color)GetValue(MatchColorProperty);
    set => SetValue(MatchColorProperty, value);
  }

  public static readonly DependencyProperty CurrentMatchColorProperty = DependencyProperty.Register("CurrentMatchColor", typeof(Color), typeof(FindReplaceBox), new PropertyMetadata(Colors.OrangeRed));
  public Color CurrentMatchColor
  {
    get => (Color)GetValue(CurrentMatchColorProperty);
    set => SetValue(CurrentMatchColorProperty, value);
  }

  public static readonly DependencyProperty IsCaseSensitiveProperty = DependencyProperty.Register("IsCaseSensitive", typeof(bool), typeof(FindReplaceBox), new PropertyMetadata(false, OnIsCaseSensitivehanged));
  public bool IsCaseSensitive
  {
    get => (bool)GetValue(IsCaseSensitiveProperty);
    set => SetValue(IsCaseSensitiveProperty, value);
  }

  public static readonly DependencyProperty IsRegexEnabledProperty = DependencyProperty.Register("IsRegexEnabled", typeof(bool), typeof(FindReplaceBox), new PropertyMetadata(false, OnIsRegexEnabledChanged));
  public bool IsRegexEnabled
  {
    get => (bool)GetValue(IsRegexEnabledProperty);
    set => SetValue(IsRegexEnabledProperty, value);
  }

  public static readonly DependencyProperty IsContextChangedProperty = DependencyProperty.Register("IsContextChanged", typeof(bool), typeof(FindReplaceBox), new PropertyMetadata(true, OnIsContextChanged));
  public bool IsContextChanged
  {
    get => (bool)GetValue(IsContextChangedProperty);
    private set => SetValue(IsContextChangedProperty, value);
  }

  private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded)
    {
      if (e.OldValue is RichEditBox oldTarget)
      {
        // 이벤트 해제
      }
      if (e.NewValue is RichEditBox newTarget)
      {
        // 이벤트 등록
      }
    }
  }

  private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded && e.NewValue is false)
    {
      control.ResetMatchResults();
      control.FindAutoSuggestBox?.Focus(FocusState.Keyboard);
    }
  }

  private static void OnFindReplaceModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded)
    {
      switch (control.FindReplaceMode)
      {
        case FindReplaceMode.Find:
          VisualStateManager.GoToState(control, "FindMode", true);
          break;
        case FindReplaceMode.Replace:
          VisualStateManager.GoToState(control, "ReplaceMode", true);
          break;
      }
    }
  }

  private static void OnFindTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded)
    {
      control.IsContextChanged = true;
      VisualStateManager.GoToState(control, "Unmatched", true);
    }
  }

  private static void OnReplaceTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
  }

  private static void OnIsCaseSensitivehanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded)
    {
      control.IsContextChanged = true;
    }
  }

  private static void OnIsRegexEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded)
    {
      control.IsContextChanged = true;
    }
  }

  private static void OnIsContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FindReplaceBox control && control.IsLoaded)
    {
      if (e.NewValue is true)
      {
        VisualStateManager.GoToState(control, "Unmatched", true);
      }
      else
      {
        VisualStateManager.GoToState(control, "Matched", true);
      }
    }
  }

  private ToggleButton? CaseSensitiveToggleButton;
  private ToggleButton? RegexToggleButton;
  private Button? CloseButton;

  private Button? ToggleFindReplaceButton;
  private AutoSuggestBox? FindAutoSuggestBox;
  private Button? FindNextButton;
  private Button? FindPreviousButton;

  private Grid? ReplaceGrid;
  private AutoSuggestBox? ReplaceAutoSuggestBox;
  private Button? ReplaceNextButton;
  private Button? ReplaceAllButton;

  private TextBlock? MatchResultTextBlock;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    CaseSensitiveToggleButton = GetTemplateChild("CaseSensitiveToggleButton") as ToggleButton;
    RegexToggleButton = GetTemplateChild("RegexToggleButton") as ToggleButton;
    CloseButton = GetTemplateChild("CloseButton") as Button;

    ToggleFindReplaceButton = GetTemplateChild("ToggleFindReplaceButton") as Button;
    FindAutoSuggestBox = GetTemplateChild("FindAutoSuggestBox") as AutoSuggestBox;
    FindNextButton = GetTemplateChild("FindNextButton") as Button;
    FindPreviousButton = GetTemplateChild("FindPreviousButton") as Button;

    ReplaceGrid = GetTemplateChild("ReplaceGrid") as Grid;
    ReplaceAutoSuggestBox = GetTemplateChild("ReplaceAutoSuggestBox") as AutoSuggestBox;
    ReplaceNextButton = GetTemplateChild("ReplaceNextButton") as Button;
    ReplaceAllButton = GetTemplateChild("ReplaceAllButton") as Button;

    MatchResultTextBlock = GetTemplateChild("MatchResultTextBlock") as TextBlock;

    CloseButton?.Click += CloseButton_Click;

    ToggleFindReplaceButton?.Click += ToggleFindReplaceButton_Click;
    ReplaceGrid?.Visibility = FindReplaceMode is FindReplaceMode.Replace ? Visibility.Visible : Visibility.Collapsed;

    FindAutoSuggestBox?.QuerySubmitted += FindAutoSuggestBox_QuerySubmitted;

    FindNextButton?.Click += FindNextButton_Click;
    FindPreviousButton?.Click += FindPreviousButton_Click;

    ReplaceAutoSuggestBox?.QuerySubmitted += ReplaceAutoSuggestBox_QuerySubmitted;
    ReplaceNextButton?.Click += ReplaceNextButton_Click;
    ReplaceAllButton?.Click += ReplaceAllButton_Click;
  }

  private readonly List<int> _matchResults = new();

  private void CloseButton_Click(object sender, RoutedEventArgs e) => IsOpen = false;

  private void ToggleFindReplaceButton_Click(object sender, RoutedEventArgs e)
  {
    FindReplaceMode = FindReplaceMode switch
    {
      FindReplaceMode.Find => FindReplaceMode.Replace,
      FindReplaceMode.Replace => FindReplaceMode.Find,
      _ => throw new ArgumentException("Invalid FindReplaceMode")
    };
  }

  private void FindAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    FindNext();
  }

  private void FindNextButton_Click(object sender, RoutedEventArgs e)
  {
    FindNext();
  }

  private void FindPreviousButton_Click(object sender, RoutedEventArgs e)
  {
    FindPrevious();
  }

  private void FindNext()
  {
    if (string.IsNullOrEmpty(FindText))
      return;

    if (IsContextChanged)
      Find(FindText);

    if (TargetEditor is RichEditBox box)
    {
      int _matchIndex = _matchResults.BinarySearch(box.Document.Selection.StartPosition);
      if (_matchIndex < 0)
        _matchIndex = ~_matchIndex;

      _matchIndex++;
      if (_matchIndex >= _matchResults.Count)
        _matchIndex = 0;

      if (_matchIndex >= 0 && _matchIndex < _matchResults.Count)
      {
        var currentMatch = _matchResults[_matchIndex];
        box.Document.Selection.SetRange(currentMatch, currentMatch + FindText.Length);
        box.Document.Selection.ScrollIntoView(PointOptions.None);
        MatchResultTextBlock?.Text = $"{_matchIndex + 1} / {_matchResults.Count}";
      }
    }
  }

  private void FindPrevious()
  {
    if (string.IsNullOrEmpty(FindText))
      return;

    if (IsContextChanged)
      Find(FindText);

    if (TargetEditor is RichEditBox box)
    {
      int _matchIndex = _matchResults.BinarySearch(box.Document.Selection.StartPosition);
      if (_matchIndex < 0)
        _matchIndex = ~_matchIndex;

      _matchIndex--;

      if (_matchIndex < 0)
        _matchIndex = _matchResults.Count - 1;

      if (_matchIndex >= 0 && _matchIndex < _matchResults.Count)
      {
        var currentMatch = _matchResults[_matchIndex];
        box.Document.Selection.SetRange(currentMatch, currentMatch + FindText.Length);
        box.Document.Selection.ScrollIntoView(PointOptions.None);
        MatchResultTextBlock?.Text = $"{_matchIndex + 1} / {_matchResults.Count}";
      }
    }
  }

  private void Find(string text)
  {
    ResetMatchResults();
    if (TargetEditor is RichEditBox box)
    {
      box.Document.GetText(TextGetOptions.AdjustCrlf, out var document);

      if (!string.IsNullOrEmpty(document))
      {
        if (IsRegexEnabled)
        {
          var regexMatches = Regex.Matches(document, text, IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

          foreach (Match regexMatch in regexMatches)
          {
            _matchResults.Add(regexMatch.Index);
          }
        }
        else
        {
          int idx = 0;
          var stringComparison = IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
          while ((idx = document.IndexOf(text, idx, stringComparison)) != -1)
          {
            _matchResults.Add(idx);
            idx += text.Length;
          }
        }
      }
    }
    IsContextChanged = false;
  }

  private void ResetMatchResults()
  {
    if (TargetEditor is RichEditBox box)
    {
      box.Document.Selection.Collapse(true);
    }
    _matchResults.Clear();
    MatchResultTextBlock?.Text = string.Empty;
    IsContextChanged = true;
  }

  private void ReplaceAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    ReplaceNext();
  }

  private void ReplaceNextButton_Click(object sender, RoutedEventArgs e)
  {
    ReplaceNext();
  }

  private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
  {
    ReplaceAll();
  }

  private void ReplaceNext()
  {

  }

  private void ReplaceAll()
  {

  }
}

public enum FindReplaceMode
{
  Find,
  Replace
}