using System;

namespace MyNotes.Services.Database.Entities;

internal interface IDatabaseEntity<T> : IEquatable<T>
{
}
