using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace CompetitiveVerifierProblem.Generator.Test;
public abstract class TestBase
{
    public class CSharpIncrementalGeneratorTest<TIncrementalGenerator> : SourceGeneratorTest<XUnitVerifier>
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        public CSharpCompilationOptions CompilationOptions { get; set; } = new(OutputKind.ConsoleApplication);
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions;
        public CSharpParseOptions ParseOptions { get; set; } = new(languageVersion: LanguageVersion.CSharp6, kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        protected override ParseOptions CreateParseOptions() => ParseOptions;
        public AnalyzerConfigOptionsProvider? AnalyzerConfigOptionsProvider { get; set; }

        protected override string DefaultFileExt => "cs";
        public override string Language => LanguageNames.CSharp;
        protected override IEnumerable<ISourceGenerator> GetSourceGenerators() => new[] { new TIncrementalGenerator().AsSourceGenerator() };
        protected override GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<ISourceGenerator> sourceGenerators)
            => CSharpGeneratorDriver.Create(
                sourceGenerators,
                project.AnalyzerOptions.AdditionalFiles,
                (CSharpParseOptions)project.ParseOptions!,
                AnalyzerConfigOptionsProvider ?? project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
    }
    public class Test : CSharpIncrementalGeneratorTest<ProblemGenerator>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60.AddPackages(ImmutableArray.Create(new PackageIdentity("Newtonsoft.Json", "13.0.2")));
        }
    }
}
