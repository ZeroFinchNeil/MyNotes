using System;

namespace MyNotes.Debugging.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
internal sealed class ReferenceTrackerAttribute : Attribute
{
}
