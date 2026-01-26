using System;

namespace MyNotes.Services.Logging.Entities;

internal interface ILogEntity<T> : IEquatable<T>
{
}
