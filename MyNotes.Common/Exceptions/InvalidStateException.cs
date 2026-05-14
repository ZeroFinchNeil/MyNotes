using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Common.Exceptions;

internal class InvalidStateException : Exception
{
  public InvalidStateException() { }
  public InvalidStateException(string message) : base(message) { }
  public InvalidStateException(string message, Exception innerException) : base(message, innerException) { }
}
