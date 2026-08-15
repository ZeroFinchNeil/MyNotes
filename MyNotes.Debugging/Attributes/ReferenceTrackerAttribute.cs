using System;

namespace MyNotes.Debugging.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ReferenceTrackerAttribute : Attribute
{
}
