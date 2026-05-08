using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace CompetitiveVerifierProblem.Generator.Test;

public abstract class TestBase
{

    public static readonly (Type sourceGeneratorType, string filename, string content)[] ConstantGeneratedSources
        = [.. ProblemGenerator.ConstantSources.Select(t => (typeof(ProblemGenerator), t.filename, t.content.ReplaceLineEndings()))];

    public class CSharpIncrementalGeneratorTest<TIncrementalGenerator> : SourceGeneratorTest<DefaultVerifier>
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        public CSharpCompilationOptions CompilationOptions { get; set; } = new(OutputKind.ConsoleApplication);
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions;
        public CSharpParseOptions ParseOptions { get; set; } = new(languageVersion: LanguageVersion.CSharp6, kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        protected override ParseOptions CreateParseOptions() => ParseOptions;
        public AnalyzerConfigOptionsProvider? AnalyzerConfigOptionsProvider { get; set; }

        protected override string DefaultFileExt => "cs";
        public override string Language => LanguageNames.CSharp;
        protected override IEnumerable<Type> GetSourceGenerators() => [typeof(TIncrementalGenerator)];
    }
    public class GeneratorTest : CSharpIncrementalGeneratorTest<ProblemGenerator>
    {
        public GeneratorTest()
        {
            TestState.AdditionalReferences.Add(typeof(CompetitiveVerifier.Core.ProblemSolverBase).Assembly);
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages([new PackageIdentity("System.Text.Json", "10.0.7")]);
        }
    }
}
