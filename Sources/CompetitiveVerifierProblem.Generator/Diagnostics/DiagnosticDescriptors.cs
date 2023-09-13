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

    public static Diagnostic VERIFY0002_WithoutDefaultConstructor(string className, Location location)
        => Diagnostic.Create(VERIFY0002_WithoutDefaultConstructor_Descriptor, location, className);
    private static readonly DiagnosticDescriptor VERIFY0002_WithoutDefaultConstructor_Descriptor = new(
        "VERIFY0002",
        new LocalizableResourceString(
            nameof(DiagnosticsResources.VERIFY0002_Title),
            DiagnosticsResources.ResourceManager,
            typeof(DiagnosticsResources)),
        new LocalizableResourceString(
            nameof(DiagnosticsResources.VERIFY0002_Body),
            DiagnosticsResources.ResourceManager,
            typeof(DiagnosticsResources)),
        "Error",
        DiagnosticSeverity.Warning,
        true);

    public static Diagnostic VERIFY0003_GenericTypeArguments(Location location)
        => Diagnostic.Create(VERIFY0003_GenericTypeArguments_Descriptor, location);
    private static readonly DiagnosticDescriptor VERIFY0003_GenericTypeArguments_Descriptor = new(
        "VERIFY0003",
        new LocalizableResourceString(
            nameof(DiagnosticsResources.VERIFY0003_Title),
            DiagnosticsResources.ResourceManager,
            typeof(DiagnosticsResources)),
        new LocalizableResourceString(
            nameof(DiagnosticsResources.VERIFY0003_Body),
            DiagnosticsResources.ResourceManager,
            typeof(DiagnosticsResources)),
        "Error",
        DiagnosticSeverity.Warning,
        true);
}
