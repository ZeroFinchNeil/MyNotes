using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using MyNotes.Common.Operations;
using MyNotes.Services.Search.Analyzers;
using MyNotes.Services.Search.Entities;

using Windows.Storage;

using IO = System.IO;

namespace MyNotes.Services.Search;

internal sealed class SearchService : IDisposable
{
  private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  public static readonly LuceneVersion LuceneVersion = LuceneVersion.LUCENE_48;
  public FSDirectory NoteSearchIndexFSDir { get; }
  public MaxGramAnalyzer NoteSearchAnalyzer { get; }
  public IndexWriter NoteSearchWriter { get; }
  public static readonly int NoteSearchMaxGram = 5;
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

  public SearchService()
  {
    var searchDirInfo = IO.Directory.CreateDirectory(IO.Path.Combine(_localFolder.Path, "Search"));
    var noteSearchDirInfo = IO.Directory.CreateDirectory(IO.Path.Combine(searchDirInfo.FullName, "Note"));
    NoteSearchIndexFSDir = FSDirectory.Open(IO.Path.Combine(noteSearchDirInfo.FullName, "index"));
    NoteSearchAnalyzer = new MaxGramAnalyzer(NoteSearchMaxGram);
    var indexConfig = new IndexWriterConfig(LuceneVersion, NoteSearchAnalyzer) { OpenMode = OpenMode.CREATE_OR_APPEND };
    NoteSearchWriter = new IndexWriter(NoteSearchIndexFSDir, indexConfig);
    _commitTimer.Elapsed += CommitTimer_Elapsed;

    _ = RunWorker();
  }

  private bool _disposed;
  public bool IsDisposed => _disposed;

  private void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        NoteSearchIndexChannel.Writer.TryComplete();
        _commitTimer.Dispose();
        NoteSearchIndexFSDir.Dispose();
        NoteSearchAnalyzer.Dispose();
        NoteSearchWriter.Dispose();
      }

      _disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  private readonly Channel<IOperationRequest> NoteSearchIndexChannel = Channel.CreateUnbounded<IOperationRequest>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

  private Task RunWorker() => Task.Run(async () =>
  {
    await foreach (IOperationRequest request in NoteSearchIndexChannel.Reader.ReadAllAsync())
    {
      request.Execute();
    }
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
        NoteSearchWriter.Commit();
    });
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }

  public async Task WriteNoteIndexAsync(NoteSearchEntity entity, CancellationToken cancellationToken = default)
  {
    var doc = new Document()
      {
        new StringField(nameof(NoteSearchEntity.Id), entity.Id.ToString(), Field.Store.YES),
        new TextField(nameof(NoteSearchEntity.Title), entity.Title, Field.Store.NO),
        new Field(nameof(NoteSearchEntity.Body), entity.Body, NoteSearchBodyFieldType)
      };

    Term term = new(nameof(NoteSearchEntity.Id), entity.Id.ToString());
    SearchIndexingOperationRequest request = new(() =>
    {
      if (!cancellationToken.IsCancellationRequested)
        NoteSearchWriter.UpdateDocument(term, doc);
    });
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    CommitCount++;
    _commitTimer.Start();
    await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }

  public async Task DeleteNoteIndexAsync(Guid id, CancellationToken cancellationToken = default)
  {
    Term term = new(nameof(NoteSearchEntity.Id), id.ToString());

    SearchIndexingOperationRequest request = new(() =>
    {
      if (!cancellationToken.IsCancellationRequested)
        NoteSearchWriter.DeleteDocuments(term);
    });
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    CommitCount++;
    _commitTimer.Start();
    await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }

  private static List<string> GetTokens(string word, int maxGram)
  {
    word = word.ToLowerInvariant();
    int length = word.Length;

    List<string> tokens = new();
    if (length <= maxGram)
      tokens.Add(word);
    else
      for (int index = 0; index <= length - maxGram; index++)
        tokens.Add(word[index..(index + maxGram)]);

    return tokens;
  }

  public async Task<NoteSearchResult?> SearchNoteIndexAsync(string searchText, CancellationToken cancellationToken = default)
  {
    var tokens = GetTokens(searchText, NoteSearchMaxGram);

    SearchIndexingOperationRequest<NoteSearchResult?> request = new(() => new NoteSearchResult()
    {
      SearchText = searchText,
      SearchTokens = tokens,
      Matches = GetIndexReaderMatches(searchText, tokens, cancellationToken)
    }, null);
    await NoteSearchIndexChannel.Writer.WriteAsync(request, cancellationToken);
    return await request.TaskCompletionSource.Task.WaitAsync(cancellationToken);
  }

  private async IAsyncEnumerable<NoteSearchTokenMatch> GetIndexReaderMatches(string searchText, IEnumerable<string> tokens, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      using DirectoryReader indexReader = NoteSearchWriter.GetReader(true);
      IndexSearcher indexSearcher = new(indexReader);
      QueryParser bodyParser = new(LuceneVersion, nameof(NoteSearchEntity.Body), NoteSearchAnalyzer);
      var bodySearchQuery = bodyParser.Parse(searchText);
      ScoreDoc? currentDoc = null;

      while (true)
      {
        var topDocs = indexSearcher.SearchAfter(currentDoc, bodySearchQuery, NoteSearchPageSize);
        var scoreDocs = topDocs.ScoreDocs;

        if (scoreDocs.Length == 0)
          break;

        foreach (var scoreDoc in scoreDocs)
        {
          var docId = scoreDoc.Doc;
          var doc = indexSearcher.Doc(docId);

          var termsEnum = indexReader.GetTermVector(docId, nameof(NoteSearchEntity.Body)).GetEnumerator();
          if (termsEnum is null)
            continue;

          var docsEnum = termsEnum.DocsAndPositions(null, null, DocsAndPositionsFlags.OFFSETS);

          List<Range> offsets = new();
          foreach (string token in tokens)
          {
            if (termsEnum.SeekExact(new BytesRef(token)))
            {
              docsEnum = termsEnum.DocsAndPositions(null, docsEnum, DocsAndPositionsFlags.OFFSETS);

              if (docsEnum is null)
                break;

              Dictionary<int, Range> currentMatches = new();

              while (docsEnum.NextDoc() != DocIdSetIterator.NO_MORE_DOCS)
              {
                for (int i = 0; i < docsEnum.Freq; i++)
                  offsets.Add(new Range(docsEnum.StartOffset, docsEnum.EndOffset));
              }
            }
          }

          yield return new NoteSearchTokenMatch()
          {
            Score = scoreDoc.Score,
            NoteId = Guid.Parse(doc.Get(nameof(NoteSearchEntity.Id))),
            DocId = docId,
            TitleMatchFrequency = 0,
            TitleMatchOffsets = [],
            BodyMatchFrequency = docsEnum?.Freq ?? 0,
            BodyMatchOffsets = [.. offsets]
          };
        }
        currentDoc = scoreDocs.Last();
      }
    }
    finally
    {

    }
  }
}