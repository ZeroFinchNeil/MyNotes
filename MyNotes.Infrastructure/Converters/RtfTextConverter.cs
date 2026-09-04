using System;
using System.Collections.Generic;

using CommunityToolkit.WinUI.Helpers;

using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;

using MyNotes.Application.Contracts.Converters;

using Windows.Storage.Streams;

namespace MyNotes.Infrastructure.Converters;

internal class RtfTextConverter : IRtfTextConverter
{
  private static RichEditBox? _richEditBox;

  public RtfTextConverter()
  {

  }

  public string ToPlainText(string? rtfText)
  {
    if (string.IsNullOrWhiteSpace(rtfText))
    {
      return string.Empty;
    }

    _richEditBox ??= new();
    var document = _richEditBox.Document;

    // 입력 문자열을 RTF로 해석합니다.
    document.SetText(TextSetOptions.FormatRtf, rtfText);

    // RTF 서식을 제외하고 일반 텍스트만 가져옵니다.
    document.GetText(TextGetOptions.UseLf, out string plainText);

    // 초기화
    document.SetText(TextSetOptions.None, string.Empty);

    return plainText;
  }

  public string ToPlainText(IRandomAccessStream randomAccessStream)
  {
    _richEditBox ??= new();
    var document = _richEditBox.Document;

    // 입력 스트림을 RTF로 해석합니다.
    document.LoadFromStream(TextSetOptions.FormatRtf, randomAccessStream);

    // RTF 서식을 제외하고 일반 텍스트만 가져옵니다.
    document.GetText(TextGetOptions.UseLf, out string plainText);

    // 초기화
    document.SetText(TextSetOptions.None, string.Empty);

    return plainText;
  }

  public string GetPreview(string? body, int start, int end)
  {
    if (string.IsNullOrWhiteSpace(body))
    {
      return string.Empty;
    }

    _richEditBox ??= new();

    var document = _richEditBox.Document;
    document.SetText(TextSetOptions.FormatRtf, body);
    document.Selection.SetRange(start, end);
    document.Selection.GetText(TextGetOptions.FormatRtf, out var preview);

    // 초기화
    document.SetText(TextSetOptions.None, string.Empty);

    return preview;
  }

  public string GetPreview(IRandomAccessStream randomAccessStream, int start, int end)
  {
    _richEditBox ??= new();

    var document = _richEditBox.Document;
    document.LoadFromStream(TextSetOptions.FormatRtf, randomAccessStream);
    document.Selection.SetRange(start, end);
    document.Selection.GetText(TextGetOptions.FormatRtf, out var preview);

    // 초기화
    document.SetText(TextSetOptions.None, string.Empty);

    return preview;
  }

  public void Highlight(ref string body, IReadOnlyList<Range> highlightRanges, string highlightColor)
  {
    _richEditBox ??= new();

    var document = _richEditBox.Document;
    document.SetText(TextSetOptions.FormatRtf, body);
    var color = ColorHelper.ToColor(highlightColor);

    var selection = document.Selection;
    int storyLength = selection.StoryLength;

    foreach (var range in highlightRanges)
    {
      if (range.Start.Value >= storyLength)
      {
        continue;
      }
      document.Selection.SetRange(0, storyLength);
      document.Selection.SetRange(range.Start.Value, range.End.Value);
      document.Selection.CharacterFormat.BackgroundColor = color;
    }

    document.Selection.SetRange(0, storyLength);
    document.Selection.GetText(TextGetOptions.FormatRtf, out body);
    // 초기화
    document.SetText(TextSetOptions.None, string.Empty);
  }
}
