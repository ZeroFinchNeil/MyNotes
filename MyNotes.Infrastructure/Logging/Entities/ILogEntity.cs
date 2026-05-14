using System;

namespace MyNotes.Infrastructure.Logging.Entities;

internal interface ILogEntity<T> : IEquatable<T>
{
}
