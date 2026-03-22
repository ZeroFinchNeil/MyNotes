using System;
using System.Windows.Input;

namespace MyNotes.Common.Commands;

public partial class Command : ICommand
{
  public Action? ActionToExecute { get; init; }
  public Func<bool>? CanExecuteFunc { get; init; }

  public Command() { }

  public Command(Action actionToExecute, Func<bool>? canExecuteFunc = null)
  {
    ActionToExecute = actionToExecute;
    CanExecuteFunc = canExecuteFunc;
  }

  public event EventHandler? CanExecuteChanged;

  public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter = null) => CanExecuteFunc?.Invoke() ?? true;

  public void Execute(object? parameter = null)
  {
    if (!CanExecute(parameter))
      return;

    ActionToExecute?.Invoke();
  }
}

public partial class Command<T> : ICommand
{
  public Action<T>? ActionToExecute { get; init; }
  public Func<T, bool>? CanExecuteFunc { get; init; }

  public Command() { }

  public Command(Action<T> actionToExecute, Func<T, bool>? canExecuteFunc = null)
  {
    ActionToExecute = actionToExecute;
    CanExecuteFunc = canExecuteFunc;
  }

  public event EventHandler? CanExecuteChanged;

  public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

  public bool CanExecute(object? parameter)
    => parameter is null || CanExecuteFunc is null || CanExecuteFunc((T)parameter);

  public void Execute(object? parameter)
  {
    if (!CanExecute(parameter))
      return;

    if (ActionToExecute is not null)
    {
      T param = parameter is null ? default! : (T)parameter;
      ActionToExecute(param);
    }
  }
}
