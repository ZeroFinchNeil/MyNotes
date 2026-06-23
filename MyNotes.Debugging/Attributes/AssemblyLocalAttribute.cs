using System;

namespace MyNotes.Debugging.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
public sealed class AssemblyLocalAttribute : Attribute
{
}