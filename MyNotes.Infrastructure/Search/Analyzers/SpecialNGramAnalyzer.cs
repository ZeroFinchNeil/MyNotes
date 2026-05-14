using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Util;

namespace MyNotes.Infrastructure.Search.Analyzers;

internal class SpecialNGramAnalyzer(LuceneVersion luceneVersion, int minGram, int maxGram) : Analyzer
{
  public LuceneVersion LuceneVersion { get; } = luceneVersion;
  public int MinGram { get; } = minGram;
  public int MaxGram { get; } = maxGram;

  protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
  {
    Tokenizer tokenizer = new NGramTokenizer(LuceneVersion, reader, MinGram, MaxGram);
    TokenStream filter = new LowerCaseFilter(LuceneVersion, tokenizer);
    return new TokenStreamComponents(tokenizer, filter);
  }

  //protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
  //{
  //  // 1. 표준 토크나이저(단어 단위)
  //  Tokenizer tokenizer = new StandardTokenizer(LuceneVersion, reader);

  //  // 2. 소문자 변환
  //  TokenStream filter = new LowerCaseFilter(LuceneVersion, tokenizer);

  //  // 3. NGramFilter 적용: 각 단어를 gram 단위로 분할
  //  filter = new SpecialNGramTokenFilter(LuceneVersion, filter, MinGram, MaxGram);

  //  return new TokenStreamComponents(tokenizer, filter);
  //}
}