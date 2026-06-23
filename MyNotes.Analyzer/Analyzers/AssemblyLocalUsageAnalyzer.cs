using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace MyNotes.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssemblyLocalUsageAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "AL0001";

  private static readonly DiagnosticDescriptor Rule = new(
      id: DiagnosticId,
      title: "Assembly-local symbol used outside declaring assembly",
      messageFormat: "'{0}' is marked with [AssemblyLocal] and should only be used within assembly '{1}'",
      category: "Usage",
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "Symbols marked with AssemblyLocalAttribute should not be used outside their declaring assembly, even when InternalsVisibleTo makes them visible.");

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
      => ImmutableArray.Create(Rule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterOperationAction(
        AnalyzeOperation,
        OperationKind.ObjectCreation,
        OperationKind.Invocation,
        OperationKind.FieldReference,
        OperationKind.PropertyReference,
        OperationKind.EventReference,
        OperationKind.MethodReference);
  }

  private static void AnalyzeOperation(OperationAnalysisContext context)
  {
    var referencedSymbol = GetReferencedSymbol(context.Operation);

    if (referencedSymbol is null)
    {
      return;
    }

    var assemblyLocalSymbol = FindAssemblyLocalSymbol(referencedSymbol);

    if (assemblyLocalSymbol is null)
    {
      return;
    }

    var declaringAssembly = assemblyLocalSymbol.ContainingAssembly;
    var currentAssembly = context.Compilation.Assembly;

    if (SymbolEqualityComparer.Default.Equals(declaringAssembly, currentAssembly))
    {
      return;
    }

    var diagnostic = Diagnostic.Create(
        Rule,
        context.Operation.Syntax.GetLocation(),
        assemblyLocalSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
        declaringAssembly.Name);

    context.ReportDiagnostic(diagnostic);
  }

  private static ISymbol? GetReferencedSymbol(IOperation operation)
  {
    return operation switch
    {
      IObjectCreationOperation objectCreation => objectCreation.Constructor?.ContainingType,
      IInvocationOperation invocation => invocation.TargetMethod,
      IFieldReferenceOperation fieldReference => fieldReference.Field,
      IPropertyReferenceOperation propertyReference => propertyReference.Property,
      IEventReferenceOperation eventReference => eventReference.Event,
      IMethodReferenceOperation methodReference => methodReference.Method,
      _ => null
    };
  }

  private static ISymbol? FindAssemblyLocalSymbol(ISymbol symbol)
  {
    if (HasAssemblyLocalAttribute(symbol))
    {
      return symbol;
    }

    var containingType = symbol.ContainingType;

    while (containingType is not null)
    {
      if (HasAssemblyLocalAttribute(containingType))
      {
        return containingType;
      }

      containingType = containingType.ContainingType;
    }

    return null;
  }

  private static bool HasAssemblyLocalAttribute(ISymbol symbol)
  {
    foreach (var attribute in symbol.GetAttributes())
    {
      var attributeClass = attribute.AttributeClass;

      if (attributeClass is null)
      {
        continue;
      }

      if (attributeClass.Name is "AssemblyLocalAttribute" or "AssemblyLocal")
      {
        return true;
      }
    }

    return false;
  }
}