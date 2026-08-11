using System;
using System.Threading.Tasks;

namespace MyNotes.Common.Operations;

public interface IAsyncOperationRequest
{
  public Task Task { get; }
  public Task ExecuteAsync();
}

public interface IAsyncOperationRequest<T> : IAsyncOperationRequest
{
  public Func<Task<T>> Operation { get; }

  public T FallbackValue { get; }

  public Task<T> Result { get; }

  Task IAsyncOperationRequest.Task => Result;
}