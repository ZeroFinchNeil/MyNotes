using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using MyNotes.Common.Operations;
using MyNotes.Infrastructure.Search.Analyzers;
using MyNotes.Infrastructure.Search.Constants;
using MyNotes.Infrastructure.Search.Documents.Notes;

using Windows.Storage;

using IO = System.IO;

namespace MyNotes.Infrastructure.Search.Core;

internal sealed class AppSearchContext : IDisposable
{
  private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  public static readonly LuceneVersion LuceneVersion = LuceneVersion.LUCENE_48;
  public FSDirectory NoteSearchIndexFSDir { get; }
  public SpecialNGramAnalyzer NoteSearchAnalyzer { get; }
  public IndexWriter NoteSearchWriter { get; }

  public static readonly int NoteSearchMinGram = 3;
  public static readonly int NoteSearchMaxGram = 7;
  public static readonly int NoteSearchPageSize = 100;
  public static readonly FieldType NoteSearchBodyFieldType = new()
  {
    IsIndexed = true,
    IsStored = false,
    IsTokenized = true,
    StoreTermVectors = true,
    StoreTermVectorPositions = true,
    StoreTermVectorOffsets = true,
  };

  public AppSearchContext()
  {
    var searchDirInfo = IO.Directory.CreateDirectory(IO.Path.Combine(_localFolder.Path, SearchCoreSettings.SearchIndexDirectoryName));
    NoteSearchIndexFSDir = FSDirectory.Open(IO.Path.Combine(searchDirInfo.FullName, SearchCoreSettings.NoteSearchIndexDirectoryName));
    NoteSearchAnalyzer = new SpecialNGramAnalyzer(LuceneVersion, NoteSearchMinGram, NoteSearchMaxGram);
    var indexConfig = new IndexWriterConfig(LuceneVersion, NoteSearchAnalyzer) { OpenMode = OpenMode.CREATE_OR_APPEND };
    NoteSearchWriter = new IndexWriter(NoteSearchIndexFSDir, indexConfig);
    _commitTimer.Elapsed += CommitTimer_Elapsed;

    _workerCompletionTask = RunWorker();
  }

  public bool Disposed { get; private set; }

  private void Dispose(bool disposing)
  {
    if (!Disposed)
    {
      if (disposing)
      {
        NoteSearchIndexChannel.Writer.TryComplete();
        _workerCompletionTask.Wait();
        _commitTimer.Dispose();
        NoteSearchIndexFSDir.Dispose();
        NoteSearchAnalyzer.Dispose();
        NoteSearchWriter.Dispose();
      }

      Disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  private readonly Channel<IOperationRequest> NoteSearchIndexChannel = Channel.CreateUnbounded<IOperationRequest>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

  private Task _workerCompletionTask;
  private Task RunWorker() => Task.Run(async () =>
  {
    await foreach (IOperationRequest request in NoteSearchIndexChannel.Reader.ReadAllAsync())
    {
      request.Execute();
    }
    NoteSearchWriter.Commit();
  });

  private static readonly int _invokeCommitCount = 20;
  private static readonly TimeSpan _invokeCommitTimeSpan = TimeSpan.FromMinutes(5);

  public int CommitCount
  {
    get;
    private set
    {
      field = value;
      if (value >= _invokeCommitCount)
      {
        NoteSearchWriter.Commit();
        field = 0;
      }
    }
  } = 0;

  private readonly System.Timers.Timer _commitTimer = new(_invokeCommitTimeSpan) { AutoReset = false };
  private void CommitTimer_Elapsed(object? sender, ElapsedEventArgs e)
  {
    _commitTimer.Stop();
    NoteSearchWriter.Commit();
  }

  public async Task CommitAsync(CancellationToken cancellationToken = default)
  {
    SearchIndexingOperationRequest request = new(() =>
    {
      if (!cancellationToken.IsCancellationRequested)
      {
        NoteSearchWriter.Commit();
      }
    });
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
    _commitTimer.Stop();
    CommitCount = 0;
  }

  #region Write And Update
  public async Task<bool> WriteNoteIndexAsync(NoteSearchDocument entity, CancellationToken cancellationToken = default)
  {
    //var doc = new Document()
    //  {
    //    new StringField(nameof(NoteSearchEntity.Id), entity.Id.ToString(), Field.Store.YES),
    //    new Field(nameof(NoteSearchEntity.Title), entity.Title, NoteSearchBodyFieldType),
    //    new Field(nameof(NoteSearchEntity.Body), entity.Body, NoteSearchBodyFieldType)
    //  };

    var doc = new Document()
    {
      new StringField(nameof(NoteSearchDocument.Id), entity.Id.ToString(), Field.Store.YES),
      new TextField(nameof(NoteSearchDocument.Title), entity.Title, Field.Store.NO),
      new TextField(nameof(NoteSearchDocument.Body), entity.Body, Field.Store.NO)
    };

    Term term = new(nameof(NoteSearchDocument.Id), entity.Id.ToString());
    if (cancellationToken.IsCancellationRequested)
    {
      return false;
    }

    SearchIndexingOperationRequest<bool> request = new(() =>
    {
      bool updated = false;
      try
      {
        NoteSearchWriter.UpdateDocument(term, doc);
        updated = true;
      }
      catch
      {

      }
      return updated;
    });

    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    CommitCount++;
    _commitTimer.Start();
    return await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }
  #endregion

  #region Delete
  public async Task DeleteNoteIndexAsync(Guid id, CancellationToken cancellationToken = default)
  {
    Term term = new(nameof(NoteSearchDocument.Id), id.ToString());

    SearchIndexingOperationRequest request = new(() =>
    {
      if (!cancellationToken.IsCancellationRequested)
      {
        NoteSearchWriter.DeleteDocuments(term);
      }
    });
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    CommitCount++;
    _commitTimer.Start();
    await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }
  #endregion

  #region Read All
  public async Task<IAsyncEnumerable<NoteSearchDocument>> ReadAllAsync(CancellationToken cancellationToken = default)
  {
    async IAsyncEnumerable<NoteSearchDocument> Search()
    {
      using DirectoryReader indexReader = NoteSearchWriter.GetReader(true);
      IndexSearcher indexSearcher = new(indexReader);
      Query query = new MatchAllDocsQuery();
      ScoreDoc? currentDoc = null;

      while (true)
      {
        TopDocs topDocs = indexSearcher.SearchAfter(currentDoc, query, NoteSearchPageSize);
        var scoreDocs = topDocs.ScoreDocs;
        if (scoreDocs.Length == 0)
        {
          break;
        }

        foreach (var scoreDoc in scoreDocs)
        {
          Document doc = indexSearcher.Doc(scoreDoc.Doc);
          NoteSearchDocument e = new()
          {
            Id = Guid.Parse(doc.Get(nameof(NoteSearchDocument.Id))),
            Title = doc.Get(nameof(NoteSearchDocument.Title)),
            Body = doc.Get(nameof(NoteSearchDocument.Body))
          };
          yield return e;
        }
        currentDoc = scoreDocs.Last();
      }
    }
    SearchIndexingOperationRequest<IAsyncEnumerable<NoteSearchDocument>> request = new(Search);

    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    return await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }
  #endregion

  #region Search Index
  public async Task<NoteSearchResult?> SearchNoteIndexAsync(string searchText, CancellationToken cancellationToken = default)
  {
    SearchIndexingOperationRequest<NoteSearchResult?> request = new(() => new NoteSearchResult()
    {
      SearchText = searchText,
      Matches = GetIndexReaderMatches(searchText, cancellationToken)
    }, null);
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    return await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }

  private async IAsyncEnumerable<NoteSearchTokenMatch> GetIndexReaderMatches(string searchText, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      using DirectoryReader indexReader = NoteSearchWriter.GetReader(true);
      IndexSearcher indexSearcher = new(indexReader);
      MultiFieldQueryParser parser = new(LuceneVersion, [nameof(NoteSearchDocument.Title), nameof(NoteSearchDocument.Body)], NoteSearchAnalyzer) { DefaultOperator = Operator.AND };
      var searchQuery = parser.Parse(searchText);
      Console.WriteLine("{0}: {1}", "SearchQuery", searchQuery);
      ScoreDoc? currentDoc = null;

      while (true)
      {
        var topDocs = indexSearcher.SearchAfter(currentDoc, searchQuery, NoteSearchPageSize);
        var scoreDocs = topDocs.ScoreDocs;

        if (scoreDocs.Length == 0)
        {
          break;
        }

        foreach (var scoreDoc in scoreDocs)
        {
          var docId = scoreDoc.Doc;
          var doc = indexSearcher.Doc(docId);

          //var matches = GetDocPositionAndOffsets(indexReader, docId, tokens);

          yield return new NoteSearchTokenMatch()
          {
            Score = scoreDoc.Score,
            NoteId = Guid.Parse(doc.Get(nameof(NoteSearchDocument.Id))),
            DocId = docId,
            TitleMatchFrequency = 0,
            TitleMatchOffsets = [],
            BodyMatchFrequency = 0,
            BodyMatchOffsets = []
          };
        }
        currentDoc = scoreDocs.Last();
      }
    }
    finally
    {

    }
  }

  private Dictionary<int, Range>? GetDocPositionAndOffsets(IndexReader indexReader, int docId, List<string> tokens)
  {
    var termsEnum = indexReader.GetTermVector(docId, "body").GetEnumerator();
    // TermsEnum: ScoreDoc의 특정 필드에서 발생한 모든 Term 나열

    Dictionary<int, Range>? matches = null;
    foreach (string token in tokens)
    {
      if (termsEnum.SeekExact(new BytesRef(token)))
      {
        var docsEnum = termsEnum.DocsAndPositions(null, null);

        if (docsEnum is null)
        {
          matches = null;
          break;
        }

        Dictionary<int, Range> currentMatches = new();

        while (docsEnum.NextDoc() != DocIdSetIterator.NO_MORE_DOCS)
        {
          for (int i = 0; i < docsEnum.Freq; i++)
          {
            currentMatches.TryAdd(docsEnum.NextPosition(), new Range(docsEnum.StartOffset, docsEnum.EndOffset));
          }
        }

        matches = matches is null
          ? currentMatches
          : matches.Where(match => currentMatches.ContainsKey(match.Key)).ToDictionary();
      }
      else
      {
        matches = null;
        break;
      }
    }

    return matches;
  }

  private static List<string> GetTokens(Analyzer analyzer, string inputText)
  {
    var tokens = new List<string>();

    using var reader = new IO.StringReader(inputText);
    using TokenStream tokenStream = analyzer.GetTokenStream(string.Empty, reader);

    tokenStream.Reset();
    while (tokenStream.IncrementToken())
    {
      var termAttr = tokenStream.GetAttribute<Lucene.Net.Analysis.TokenAttributes.ICharTermAttribute>();
      tokens.Add(termAttr.ToString());
    }
    tokenStream.End();
    return tokens;
  }

  private static List<string> GetTokens(string word, int maxGram)
  {
    word = word.ToLowerInvariant();
    int length = word.Length;

    List<string> tokens = new();
    if (length <= maxGram)
    {
      tokens.Add(word);
    }
    else
    {
      for (int index = 0; index <= length - maxGram; index++)
      {
        tokens.Add(word[index..(index + maxGram)]);
      }
    }

    return tokens;
  }
  #endregion
}