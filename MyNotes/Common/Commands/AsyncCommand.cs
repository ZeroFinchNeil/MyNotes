using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace MyNotes.Common.Commands;

public sealed partial class AsyncCommand : ICommand
{
  public required Func<Task> ExecuteFunc { get; init; }
  public Func<bool>? CanExecuteFunc { get; init; }
  public bool AllowsConcurrentExecutions { get; init; } = false;

  public AsyncCommand() { }

  [SetsRequiredMembers]
  public AsyncCommand(Func<Task> executeFunc, Func<bool>? canExecuteFunc = null, bool allowsConcurrentExecutions = false)
  {
    ExecuteFunc = executeFunc;
    CanExecuteFunc = canExecuteFunc;
    AllowsConcurrentExecutions = allowsConcurrentExecutions;
  }

  public event EventHandler? CanExecuteChanged;

  public bool IsExecuting
  {
    get;
    private set
    {
      if (field == value)
      {
        return;
      }

      field = value;
      NotifyCanExecuteChanged();
    }
  }

  public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter = null)
  {
    if (!AllowsConcurrentExecutions && IsExecuting)
    {
      return false;
    }

    return CanExecuteFunc?.Invoke() ?? true;
  }

  public async void Execute(object? parameter = null) => await ExecuteAsync(parameter);

  public async Task ExecuteAsync(object? parameter = null)
  {
    if (!CanExecute(parameter))
    {
      return;
    }

    try
    {
      IsExecuting = true;
      await ExecuteFunc();
    }
    finally
    {
      IsExecuting = false;
    }
  }
}

public sealed partial class AsyncCommand<T> : ICommand
{
  public required Func<T, Task> ExecuteFunc { get; init; }
  public Func<T, bool>? CanExecuteFunc { get; init; }
  public bool AllowsConcurrentExecutions { get; init; } = false;

  public AsyncCommand() { }

  [SetsRequiredMembers]
  public AsyncCommand(Func<T, Task> executeFunc, Func<T, bool>? canExecuteFunc = null, bool allowsConcurrentExecutions = false)
  {
    ExecuteFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
    CanExecuteFunc = canExecuteFunc;
    AllowsConcurrentExecutions = allowsConcurrentExecutions;
  }

  public event EventHandler? CanExecuteChanged;

  public bool IsExecuting
  {
    get;
    private set
    {
      if (field == value)
      {
        return;
      }

      field = value;
      NotifyCanExecuteChanged();
    }
  }

  public bool CanExecute(object? parameter)
  {
    if (!AllowsConcurrentExecutions && IsExecuting)
    {
      return false;
    }

    return parameter is T value && (CanExecuteFunc?.Invoke(value) ?? true);
  }

  public async void Execute(object? parameter)
  {
    if (parameter is not T value)
    {
      return;
    }

    await ExecuteAsync(value);
  }

  public async Task ExecuteAsync(T parameter)
  {
    if (!CanExecute(parameter))
    {
      return;
    }

    try
    {
      IsExecuting = true;
      await ExecuteFunc(parameter);
    }
    finally
    {
      IsExecuting = false;
    }
  }

  public void NotifyCanExecuteChanged()
  {
    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
  }
}