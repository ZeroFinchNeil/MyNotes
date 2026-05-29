using System;
using System.Windows.Input;

namespace MyNotes.Common.Commands;

public sealed partial class Command : ICommand
{
  public Action? ExecuteAction { get; init; }
  public Func<bool>? CanExecuteFunc { get; init; }

  public Command() { }

  public Command(Action executeAction, Func<bool>? canExecuteFunc = null)
  {
    ExecuteAction = executeAction;
    CanExecuteFunc = canExecuteFunc;
  }

  public event EventHandler? CanExecuteChanged;

  public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter = null) => CanExecuteFunc?.Invoke() ?? true;

  public void Execute(object? parameter = null)
  {
    if (!CanExecute(parameter))
    {
      return;
    }

    ExecuteAction?.Invoke();
  }
}

public sealed partial class Command<T> : ICommand
{
  public Action<T>? ExecuteAction { get; init; }
  public Func<T, bool>? CanExecuteFunc { get; init; }

  public Command() { }

  public Command(Action<T> executeAction, Func<T, bool>? canExecuteFunc = null)
  {
    ExecuteAction = executeAction;
    CanExecuteFunc = canExecuteFunc;
  }

  public event EventHandler? CanExecuteChanged;

  public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter)
    => parameter is null || CanExecuteFunc is null || CanExecuteFunc((T)parameter);

  public void Execute(object? parameter)
  {
    if (!CanExecute(parameter))
    {
      return;
    }

    if (ExecuteAction is not null)
    {
      T param = parameter is null ? default! : (T)parameter;
      ExecuteAction(param);
    }
  }
}