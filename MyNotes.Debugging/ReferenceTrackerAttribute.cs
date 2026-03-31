using System;

namespace MyNotes.Debugging;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
internal sealed class ReferenceTrackerAttribute : Attribute
{
}
