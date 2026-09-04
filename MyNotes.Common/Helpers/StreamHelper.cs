using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace MyNotes.Common.Helpers;

public static class StreamHelper
{
  /// <summary>
  /// IRandomAccessStream의 전체 내용을 바이트 배열로 변환합니다.
  /// 원본 스트림의 Position은 변경하지 않습니다.
  /// </summary>
  public static async Task<byte[]> ToByteArrayAsync(IRandomAccessStream source, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(source);

    if (source.Size > int.MaxValue)
    {
      throw new IOException(
          "스트림이 너무 커서 하나의 byte 배열로 변환할 수 없습니다.");
    }

    // 원본 Position과 관계없이 처음부터 읽습니다.
    using IInputStream inputStream = source.GetInputStreamAt(0);
    using Stream managedStream = inputStream.AsStreamForRead();
    using var destination = new MemoryStream(checked((int)source.Size));

    await managedStream.CopyToAsync(destination, cancellationToken);

    return destination.ToArray();
  }

  /// <summary>
  /// 바이트 배열을 독립적인 메모리 기반 IRandomAccessStream으로 변환합니다.
  /// 반환된 스트림은 호출자가 Dispose해야 합니다.
  /// </summary>
  public static async Task<IRandomAccessStream> ToRandomAccessStreamAsync(byte[] source)
  {
    ArgumentNullException.ThrowIfNull(source);

    var result = new InMemoryRandomAccessStream();

    try
    {
      using var writer = new DataWriter(result);

      writer.WriteBytes(source);
      await writer.StoreAsync();
      await writer.FlushAsync();

      // writer가 반환할 result까지 닫지 않도록 연결을 분리합니다.
      writer.DetachStream();

      // 이후 소비자가 처음부터 읽을 수 있도록 위치를 초기화합니다.
      result.Seek(0);

      return result;
    }
    catch
    {
      result.Dispose();
      throw;
    }
  }
}