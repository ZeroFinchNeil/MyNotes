using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Util;

namespace MyNotes.Services.Search.Analyzers;

internal class MaxGramAnalyzer(int maxGram) : Analyzer
{
  public int MaxGram { get; } = maxGram;

  protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
  {
    MaxGramTokenizer tokenizer = new(reader, MaxGram);
    TokenStream tokenStream = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);

    return new TokenStreamComponents(tokenizer, tokenStream);
  }
}