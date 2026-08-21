using System;
using System.Collections.Generic;

namespace MyNotes.Application.Contracts.Converters;

public interface IRtfTextConverter
{
  public string ToPlainText(string? rtfText);

  public string GetPreview(string? body, int start, int end);

  public void Highlight(ref string body, IReadOnlyList<Range> highlightRanges, string highlightColor);
}
