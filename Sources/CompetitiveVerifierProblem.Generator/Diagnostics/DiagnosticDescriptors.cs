using Microsoft.CodeAnalysis;

namespace CompetitiveVerifierProblem.Diagnostics;
public static class DiagnosticDescriptors
{
    public static Diagnostic VERIFY0001_ShouldBeConsoleApp()
        => Diagnostic.Create(VERIFY0001_ShouldBeConsoleApp_Descriptor, Location.None);
    private static readonly DiagnosticDescriptor VERIFY0001_ShouldBeConsoleApp_Descriptor = new(
        "VERIFY0001",
        new LocalizableResourceString(
            nameof(DiagnosticsResources.VERIFY0001_Title),
            DiagnosticsResources.ResourceManager,
            typeof(DiagnosticsResources)),
        new LocalizableResourceString(
            nameof(DiagnosticsResources.VERIFY0001_Body),
            DiagnosticsResources.ResourceManager,
            typeof(DiagnosticsResources)),
        "Error",
        DiagnosticSeverity.Warning,
        true);
}
