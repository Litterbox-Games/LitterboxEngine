using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace Common.Core;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: "LB001",
        title: "Attributes are mutually exclusive",
        messageFormat: "The attributes '{0}' and '{1}' cannot be used together.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {   
        var attributes = context.Symbol.GetAttributes();
        
        var hasGame = attributes.Any(attribute => attribute.AttributeClass?.Name == "GameAttribute");
        var hasEngine = attributes.Any(attribute => attribute.AttributeClass?.Name == "EngineAttribute");
        var hasMultiplayer = attributes.Any(attribute => attribute.AttributeClass?.Name == "MultiplayerAttribute");

        var location = context.Symbol.Locations.FirstOrDefault();
        if (hasGame && hasEngine)
        {
            var diagnostic = Diagnostic.Create(Rule, location, "Game", "Engine");
            context.ReportDiagnostic(diagnostic);    
        }

        if (hasEngine && hasMultiplayer)
        {
            var diagnostic = Diagnostic.Create(Rule, location, "Engine", "Multiplayer");
            context.ReportDiagnostic(diagnostic);    
        }
    }
}