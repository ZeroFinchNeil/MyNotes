using System.Collections.Generic;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;

namespace MyNotes.Services.Search.Analyzers;

internal sealed class SpecialNGramTokenFilter : TokenFilter
{
  public readonly LuceneVersion _luceneVersion;
  private readonly int _minGram;
  private readonly int _maxGram;

  private readonly ICharTermAttribute _termAttr;
  private readonly IOffsetAttribute _offsetAttr;
  private readonly IPositionIncrementAttribute _posIncAttr;

  // NGram 출력 대기 큐
  private readonly Queue<(string token, int startOffset, int endOffset, int positionInc)> _ngramQueue = new Queue<(string, int, int, int)>();
  private State? _currentState = null;

  // 짧은 gram (minGram 미만)을 문서 내 중복검사용
  // key: gram 크기 (1~minGram-1), value: 등장한 gram HashSet
  private readonly Dictionary<int, HashSet<string>> _shortGramUniq = new Dictionary<int, HashSet<string>>();

  public SpecialNGramTokenFilter(LuceneVersion luceneVersion, TokenStream input, int minGram, int maxGram) : base(input)
  {
    _luceneVersion = luceneVersion;
    _minGram = minGram;
    _maxGram = maxGram;

    _termAttr = AddAttribute<ICharTermAttribute>();
    _offsetAttr = AddAttribute<IOffsetAttribute>();
    _posIncAttr = AddAttribute<IPositionIncrementAttribute>();

    for (int g = 1; g < _minGram; g++)
      _shortGramUniq[g] = new HashSet<string>();
  }

  public override bool IncrementToken()
  {
    if (_ngramQueue.Count > 0)
    {
      RestoreState(_currentState);
      var (token, start, end, posInc) = _ngramQueue.Dequeue();
      _termAttr.SetEmpty().Append(token);
      _offsetAttr.SetOffset(start, end);
      _posIncAttr.PositionIncrement = posInc;
      return true;
    }

    if (!m_input.IncrementToken())
      return false;

    string term = _termAttr.ToString();
    int startOffset = _offsetAttr.StartOffset;
    int termLen = term.Length;

    // 1. minGram 미만 ngram(짧은 gram): 문서 내 최초 1회만 토큰화
    for (int g = 1; g < _minGram && g <= termLen; g++)
    {
      for (int i = 0; i <= termLen - g; i++)
      {
        string gram = term.Substring(i, g);
        if (_shortGramUniq[g].Add(gram)) // HashSet에 존재하지 않으면 true
        {
          int ngStart = startOffset + i;
          int ngEnd = ngStart + g;
          // 첫 gram만 posInc 유지, 나머지는 0
          int inc = (g == 1 && i == 0) ? _posIncAttr.PositionIncrement : 0;
          _ngramQueue.Enqueue((gram, ngStart, ngEnd, inc));
        }
      }
    }

    // 2. minGram 이상은 모든 ngram 생성 (NGramTokenFilter 기본동작)
    bool isFirst = (_ngramQueue.Count == 0); // 아무 gram도 없다면 원본 posInc 사용
    for (int g = _minGram; g <= _maxGram && g <= termLen; g++)
    {
      for (int i = 0; i <= termLen - g; i++)
      {
        string gram = term.Substring(i, g);
        int ngStart = startOffset + i;
        int ngEnd = ngStart + g;
        int inc = isFirst ? _posIncAttr.PositionIncrement : 0;
        _ngramQueue.Enqueue((gram, ngStart, ngEnd, inc));
        isFirst = false;
      }
    }

    if (_ngramQueue.Count == 0)
      return IncrementToken();

    _currentState = CaptureState();
    var (firstToken, firstStart, firstEnd, firstPosInc) = _ngramQueue.Dequeue();
    _termAttr.SetEmpty().Append(firstToken);
    _offsetAttr.SetOffset(firstStart, firstEnd);
    _posIncAttr.PositionIncrement = firstPosInc;

    return true;
  }

  public override void Reset()
  {
    base.Reset();
    _ngramQueue.Clear();
    _currentState = null;
    foreach (var set in _shortGramUniq.Values)
      set.Clear();
  }
}

