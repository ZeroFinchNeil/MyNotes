using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace MyNotes.Services.Search.Analyzers;

internal class NGramAnalyzer(int minGram, int maxGram) : Analyzer
{
  public int MinGram { get; } = minGram;
  public int MaxGram { get; } = maxGram;

  protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
  {
    StandardTokenizer tokenizer = new(LuceneVersion.LUCENE_48, reader);
    TokenStream tokenStream = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);
    tokenStream = new NGramTokenFilter(LuceneVersion.LUCENE_48, tokenStream, MinGram, MaxGram);

    return new TokenStreamComponents(tokenizer, tokenStream);
  }
}
