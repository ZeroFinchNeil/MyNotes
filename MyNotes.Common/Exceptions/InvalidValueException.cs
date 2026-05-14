using System;

namespace MyNotes.Common.Exceptions;

internal class InvalidValueException : Exception
{
  public InvalidValueException() { }
  public InvalidValueException(string message) : base(message) { }
  public InvalidValueException(string message, Exception innerException) : base(message, innerException) { }

  public string? VariableName { get; }
  public object? ActualValue { get; }

  public InvalidValueException(string message, string variableName, object actualValue) : base(FormatMessage(message, variableName, actualValue))
  {
    VariableName = variableName;
    ActualValue = actualValue;
  }

  private static string FormatMessage(string message, string? variableName, object? actualValue)
  {
    if (variableName is not null)
    {
      message += $" (변수명: {variableName}";
    }

    if (actualValue is not null)
    {
      message += $"{(variableName is not null ? ", " : " (")}값: {actualValue}";
    }

    if (variableName is not null || actualValue is not null)
    {
      message += ")";
    }

    return message;
  }
}
