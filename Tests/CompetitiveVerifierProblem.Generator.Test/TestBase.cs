using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace CompetitiveVerifierProblem.Generator.Test;
public abstract class TestBase
{

    public static readonly (Type sourceGeneratorType, string filename, string content)[] ConstantGeneratedSources = new[]
        {
                        (typeof(ProblemGenerator), "ProblemSolver.cs", """
                        #pragma warning disable IDE0161,CS8602
                        namespace CompetitiveVerifier
                        {
                            using Newtonsoft.Json;

                            internal abstract class ProblemSolver
                            {
                                public abstract string Url { get; }
                                public virtual double? Error => null;
                                public virtual double? Tle => null;

                                public abstract void Solve();
                                public string ToJson()
                                {
                                    return JsonConvert.SerializeObject(new JsonDataContract
                                    {
                                        Type = "problem",
                                        Url = Url,
                                        Command = $"dotnet {System.Reflection.Assembly.GetEntryAssembly().Location} {GetType().FullName}",
                                        Error = Error,
                                        Tle = Tle,
                                    }, Formatting.None);
                                }
                                [JsonObject]
                                private struct JsonDataContract
                                {
                                    [JsonProperty("type", Required = Required.DisallowNull)]
                                    public string Type { set; get; }
                                    [JsonProperty("problem", Required = Required.DisallowNull)]
                                    public string Url { set; get; }
                                    [JsonProperty("command", Required = Required.DisallowNull)]
                                    public string Command { set; get; }
                                    [JsonProperty("error", Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
                                    public double? Error { set; get; }
                                    [JsonProperty("tle", Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
                                    public double? Tle { set; get; }
                                }
                            }
                        }
                        """.ReplaceLineEndings()),
                        (typeof(ProblemGenerator), "Main.cs", """
                        internal partial class Program
                        {
                            static void Main(string[] args)
                            {
                                if (args.Length > 0)
                                {
                                    Run(args[0]);
                                }
                                else
                                {
                                    Enumerate();
                                }
                            }
                            static partial void Run(string className);
                            static partial void Enumerate();
                        }
                        """.ReplaceLineEndings()),
    };


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
