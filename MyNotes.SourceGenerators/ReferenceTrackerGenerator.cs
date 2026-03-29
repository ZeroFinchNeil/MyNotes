using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MyNotes.SourceGenerators;

[Generator(LanguageNames.CSharp)]
internal class ReferenceTrackerGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    var trackedClasses = context.SyntaxProvider.CreateSyntaxProvider(
      predicate: (node, _) =>
        node is ClassDeclarationSyntax cds
        && cds.AttributeLists.Count > 0
        && cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
      transform: (ctx, _) =>
      {
        var classNode = (ClassDeclarationSyntax)ctx.Node;
        var model = ctx.SemanticModel;
        if (model.GetDeclaredSymbol(classNode) is INamedTypeSymbol symbol)
        {
          // [ReferenceTracker] 특성 여부 확인
          var attr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "ReferenceTrackerAttribute");
          if (attr is not null)
          {
            return symbol;
          }
        }
        return null;
      }
    ).Where(x => x is not null);

    context.RegisterSourceOutput(trackedClasses, (spc, classSymbol) =>
    {
      if (classSymbol is INamedTypeSymbol symbol)
      {
        var ns = symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString();
        var className = symbol.Name;

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(ns))
        {
          sb.AppendLine($"namespace {ns};");
        }

        // partial class 생성
        sb.AppendLine($"partial class {className}");
        sb.AppendLine("""
          {
            private void TrackReference()
            {
              if (System.Diagnostics.Debugger.IsAttached)
              {
                MyNotes.Debugging.ReferenceTracker.Register(this);
              }
            }
          }
          """);

        spc.AddSource($"{className}.ReferenceTracker.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
      }
    });
  }
}
