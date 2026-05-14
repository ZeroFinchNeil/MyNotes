using System;

namespace MyNotes.Infrastructure.Database.Entities;

internal interface IDatabaseEntity<T> : IEquatable<T>
{
}
