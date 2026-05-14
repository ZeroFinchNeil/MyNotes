using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Common.Exceptions;

internal class InvalidArgumentValueException : ArgumentException
{
  public InvalidArgumentValueException() { }

  public InvalidArgumentValueException(string message) : base(message) { }

  public InvalidArgumentValueException(string message, Exception innerException) : base(message, innerException) { }

  public InvalidArgumentValueException(string message, string paramName) : base(message, paramName) { }
}
