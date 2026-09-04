using System;
using System.Collections.Generic;

using Windows.Storage.Streams;

namespace MyNotes.Application.Contracts.Converters;

public interface IRtfTextConverter
{
  public string ToPlainText(string? rtfText);

  public string ToPlainText(IRandomAccessStream randomAccessStream);

  public string GetPreview(string? body, int start, int end);

  public string GetPreview(IRandomAccessStream randomAccessStream, int start, int end);


  public void Highlight(ref string body, IReadOnlyList<Range> highlightRanges, string highlightColor);
}
