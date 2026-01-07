using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

  public static readonly DependencyProperty FindTextProperty = DependencyProperty.Register("FindText", typeof(string), typeof(FindReplaceBox), new PropertyMetadata(string.Empty));
  public string FindText
  {
    get => (string)GetValue(FindTextProperty);
    private set => SetValue(FindTextProperty, value);
  }

  public static readonly DependencyProperty ReplaceTextProperty = DependencyProperty.Register("ReplaceText", typeof(string), typeof(FindReplaceBox), new PropertyMetadata(string.Empty));
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

  public static readonly DependencyProperty IsCaseSensitiveProperty = DependencyProperty.Register("IsCaseSensitive", typeof(bool), typeof(FindReplaceBox), new PropertyMetadata(false));
  public bool IsCaseSensitive
  {
    get => (bool)GetValue(IsCaseSensitiveProperty);
    set => SetValue(IsCaseSensitiveProperty, value);
  }

  public static readonly DependencyProperty IsRegexEnabledProperty = DependencyProperty.Register("IsRegexEnabled", typeof(bool), typeof(FindReplaceBox), new PropertyMetadata(false));
  public bool IsRegexEnabled
  {
    get => (bool)GetValue(IsRegexEnabledProperty);
    set => SetValue(IsRegexEnabledProperty, value);
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

  private Button? CloseButton;

  private Button? ToggleFindReplaceButton;
  private AutoSuggestBox? FindAutoSuggestBox;
  private Button? FindNextButton;
  private Button? FindPreviousButton;

  private Grid? ReplaceGrid;
  private AutoSuggestBox? ReplaceAutoSuggestBox;
  private Button? ReplaceNextButton;
  private Button? ReplaceAllButton;

  protected override void OnApplyTemplate()
  {
    CloseButton = GetTemplateChild("CloseButton") as Button;

    ToggleFindReplaceButton = GetTemplateChild("ToggleFindReplaceButton") as Button;
    FindAutoSuggestBox = GetTemplateChild("FindAutoSuggestBox") as AutoSuggestBox;
    FindNextButton = GetTemplateChild("FindNextButton") as Button;
    FindPreviousButton = GetTemplateChild("FindPreviousButton") as Button;

    ReplaceGrid = GetTemplateChild("ReplaceGrid") as Grid;
    ReplaceAutoSuggestBox = GetTemplateChild("ReplaceAutoSuggestBox") as AutoSuggestBox;
    ReplaceNextButton = GetTemplateChild("ReplaceNextButton") as Button;
    ReplaceAllButton = GetTemplateChild("ReplaceAllButton") as Button;

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

  private readonly List<FindReplaceBoxMatchResult> _matchResults = new();
  private int _matchIndex = -1;

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
    var newText = args.QueryText ?? string.Empty;
    if (FindText != newText)
    {
      FindText = newText;
      _matchIndex = -1;
    }

    FindNext();
  }

  private void FindNextButton_Click(object sender, RoutedEventArgs e)
  {
    var newText = FindAutoSuggestBox?.Text ?? string.Empty;
    if (FindText != newText)
    {
      FindText = newText;
      _matchIndex = -1;
    }

    FindNext();
  }

  private void FindPreviousButton_Click(object sender, RoutedEventArgs e)
  {
    var newText = FindAutoSuggestBox?.Text ?? string.Empty;
    if (FindText != newText)
    {
      FindText = newText;
      _matchIndex = -1;
    }

    FindPrevious();
  }

  private void FindNext()
  {
    if (string.IsNullOrEmpty(FindText))
      return;

    if (_matchIndex < 0)
      Find(FindText);

    if (TargetEditor is RichEditBox box)
    {
      if (_matchIndex >= 0 && _matchIndex < _matchResults.Count)
      {
        var previousMatch = _matchResults[_matchIndex];
        box.Document.GetRange(previousMatch.Start, previousMatch.End).CharacterFormat.BackgroundColor = MatchColor;
      }

      _matchIndex++;
      if (_matchIndex == _matchResults.Count)
        _matchIndex = 0;

      if (_matchIndex >= 0 && _matchIndex < _matchResults.Count)
      {
        var currentMatch = _matchResults[_matchIndex];
        box.Document.GetRange(currentMatch.Start, currentMatch.End).CharacterFormat.BackgroundColor = CurrentMatchColor;
      }
    }
  }

  private void FindPrevious()
  {
    if (string.IsNullOrEmpty(FindText))
      return;

    if (_matchIndex < 0)
      Find(FindText);

    if (TargetEditor is RichEditBox box)
    {
      if (_matchIndex >= 0 && _matchIndex < _matchResults.Count)
      {
        var previousMatch = _matchResults[_matchIndex];
        box.Document.GetRange(previousMatch.Start, previousMatch.End).CharacterFormat.BackgroundColor = MatchColor;
      }

      _matchIndex--;
      if (_matchIndex < 0)
        _matchIndex = _matchResults.Count - 1;

      if (_matchIndex >= 0 && _matchIndex < _matchResults.Count)
      {
        var currentMatch = _matchResults[_matchIndex];
        box.Document.GetRange(currentMatch.Start, currentMatch.End).CharacterFormat.BackgroundColor = CurrentMatchColor;
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
            int start = regexMatch.Index;
            int end = regexMatch.Index + regexMatch.Length;
            var textRange = box.Document.GetRange(start, end);
            var characterFormat = textRange.CharacterFormat;
            FindReplaceBoxMatchResult matchResult = new()
            {
              Start = start,
              End = end,
              CharacterFormat = characterFormat.GetClone()
            };

            characterFormat.BackgroundColor = MatchColor;
            _matchResults.Add(matchResult);
          }
        }
        else
        {
          int idx = 0;
          while ((idx = document.IndexOf(text, idx, IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
          {
            int start = idx;
            int end = idx + text.Length;
            var textRange = box.Document.GetRange(start, end);
            var characterFormat = textRange.CharacterFormat;
            FindReplaceBoxMatchResult matchResult = new()
            {
              Start = start,
              End = end,
              CharacterFormat = characterFormat.GetClone()
            };

            characterFormat.BackgroundColor = MatchColor;
            _matchResults.Add(matchResult);

            idx = end;
          }
        }
      }
    }
  }

  private void ResetMatchResults()
  {
    if (TargetEditor is RichEditBox box)
    {
      foreach (var matchResult in _matchResults)
      {
        var textRange = box.Document.GetRange(matchResult.Start, matchResult.End);
        if (textRange.Length > 0)
          textRange.CharacterFormat.SetClone(matchResult.CharacterFormat);
      }
    }
    _matchResults.Clear();
    _matchIndex = -1;
  }

  private void ReplaceAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    var newText = args.QueryText ?? string.Empty;
    if (ReplaceText != newText)
    {
      ReplaceText = newText;
    }

    ReplaceNext();
  }

  private void ReplaceNextButton_Click(object sender, RoutedEventArgs e)
  {
    var newText = ReplaceAutoSuggestBox?.Text ?? string.Empty;
    if (ReplaceText != newText)
    {
      ReplaceText = newText;
    }

    ReplaceNext();
  }

  private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
  {
    var newText = ReplaceAutoSuggestBox?.Text ?? string.Empty;
    if (ReplaceText != newText)
    {
      ReplaceText = newText;
    }

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

public record FindReplaceBoxMatchResult
{
  public required int Start { get; init; }
  public required int End { get; init; }
  public required ITextCharacterFormat CharacterFormat { get; init; }
}