using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;

using MyNotes.Shell.Contracts.Converters;

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

    // 입력 문자열을 RTF로 해석합니다.
    _richEditBox.Document.SetText(TextSetOptions.FormatRtf, rtfText);

    // RTF 서식을 제외하고 일반 텍스트만 가져옵니다.
    _richEditBox.Document.GetText(TextGetOptions.UseCrlf, out string plainText);

    // 초기화
    _richEditBox.Document.SetText(TextSetOptions.None, string.Empty);
    
    return plainText;
  }

  public string GetPreview(string? body, int start, int end)
  {
    if (string.IsNullOrWhiteSpace(body))
    {
      return string.Empty;
    }

    _richEditBox ??= new();

    _richEditBox.Document.SetText(TextSetOptions.FormatRtf, body);
    _richEditBox.Document.Selection.SetRange(start, end);
    _richEditBox.Document.Selection.GetText(TextGetOptions.FormatRtf, out var preview);

    // 초기화
    _richEditBox.Document.SetText(TextSetOptions.None, string.Empty);
    return preview;
  }
}
