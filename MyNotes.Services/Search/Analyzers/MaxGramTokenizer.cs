using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace MyNotes.Services.Search.Analyzers;

internal sealed class MaxGramTokenizer : Tokenizer
{
  public int MaxGram { get; }
  private readonly ICharTermAttribute _termAttr;
  private readonly IOffsetAttribute _offsetAttr;

  private string _text = "";
  private int _index = 0;

  public MaxGramTokenizer(TextReader input, int maxGram) : base(input)
  {
    MaxGram = maxGram;
    _termAttr = AddAttribute<ICharTermAttribute>();
    _offsetAttr = AddAttribute<IOffsetAttribute>();
  }

  public override bool IncrementToken()
  {
    if (_index == 0 && string.IsNullOrEmpty(_text))
      _text = m_input.ReadToEnd();

    if (_text.Length < MaxGram)
    {
      if (_index > 0)
        return false;

      ClearAttributes();
      _termAttr.SetEmpty().Append(_text);
      _offsetAttr.SetOffset(CorrectOffset(0), CorrectOffset(_text.Length));
      _index++;
      return true;
    }

    if (_index > _text.Length - MaxGram)
      return false;

    ClearAttributes();

    string token = _text.Substring(_index, MaxGram);
    _termAttr.SetEmpty().Append(token);
    _offsetAttr.SetOffset(CorrectOffset(_index), CorrectOffset(_index + MaxGram));

    _index++;
    return true;
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
  }
}