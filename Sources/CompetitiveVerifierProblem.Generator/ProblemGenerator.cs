using CompetitiveVerifierProblem.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CompetitiveVerifierProblem;

[Generator]
public partial class ProblemGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialization);
        context.RegisterSourceOutput(context.CompilationProvider, (ctx, compilation) =>
        {
            if (compilation.Options.OutputKind != OutputKind.ConsoleApplication)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.VERIFY0001_ShouldBeConsoleApp());
            }
        });

        var baseSolver = context.CompilationProvider
            .Select(static (compilation, token) =>
            {
                token.ThrowIfCancellationRequested();
                return compilation.GetTypeByMetadataName("CompetitiveVerifier.ProblemSolver");
            })
            .WithComparer(SymbolEqualityComparer.Default);

        var classessAndDiagnostics = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, token) => node is ClassDeclarationSyntax classDec && classDec.BaseList is not null,
                static (context, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    var syntax = (ClassDeclarationSyntax)context.Node;
                    return context.SemanticModel.GetDeclaredSymbol(syntax, token) as INamedTypeSymbol;
                }
            )
            .Collect()
            .Combine(baseSolver)
            .Select((tup, token) =>
            {
                token.ThrowIfCancellationRequested();
                var (decs, baseSolver) = tup;
                var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
                foreach (var symbol in decs)
                {
                    if (symbol is null) continue;
                    if (GetBaseTypes(symbol).Contains(baseSolver, SymbolEqualityComparer.Default))
                    {
                        builder.Add(symbol);
                    }
                }
                return builder.ToImmutable();
            });

        context.RegisterImplementationSourceOutput(classessAndDiagnostics, ImplementationSource);

        static IEnumerable<ITypeSymbol> GetBaseTypes(ITypeSymbol type)
        {
            for (var current = type?.BaseType; current is not null; current = current.BaseType)
            {
                yield return current;
            }
        }
    }

    private void ImplementationSource(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> classes)
    {
        var classesCallToJson = new StringBuilder();
        var solverSelector = new StringBuilder();
        foreach (var s in classes)
        {
            var c = s.ToDisplayString();

            if (s.IsAbstract) continue;
            if (!s.Constructors.Select(c => c.Parameters.Length).Contains(0))
            {
                foreach (var location in s.DeclaringSyntaxReferences.Select(r => Location.Create(r.SyntaxTree, r.Span)))
                {
                    context.ReportDiagnostic(DiagnosticDescriptors.VERIFY0002_WithoutDefaultConstructor(c, location));
                }
                continue;
            }

            solverSelector.Append("case \"").Append(c).Append("\":return new ").Append(c).AppendLine("();");
            classesCallToJson.AppendLine($"new {c}(),");
        }

        context.AddSource("Main.impl.cs", $$$"""
            internal partial class Program
            {
                static partial void Enumerate()
                {
                    var classes = new CompetitiveVerifier.ProblemSolver[]
                    {
            {{{classesCallToJson}}}
                    };

                    bool isFirst = true;
                    System.Console.Write('{');
                    foreach(var c in classes)
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            System.Console.Write(',');
                        System.Console.Write('"');
                        System.Console.Write(c.GetType().FullName);
                        System.Console.Write('"');
                        System.Console.Write(':');
                        System.Console.Write('[');
                        System.Console.Write(c.ToJson());
                        System.Console.Write(']');
                    }
                    System.Console.WriteLine('}');
                }

                static partial void Run(string className)
                {
                    GetSolver(className).Solve();
                }
            
                static CompetitiveVerifier.ProblemSolver GetSolver(string className)
                {
                    switch(className)
                    {
            {{{solverSelector}}}
                        default: throw new System.ArgumentException($"{className} is not found. notice: CompetitiveVerifier require FullName as argument.", nameof(className));
                    }
                }
            }
            """);
    }

    private void PostInitialization(IncrementalGeneratorPostInitializationContext context)
    {
        var token = context.CancellationToken;
        token.ThrowIfCancellationRequested();

        foreach (var (name, source) in ConstantSources)
        {
            context.AddSource(name, source);
        }
    }


    public static (string filename, string content)[] ConstantSources => new[]
        {
                        ("ProblemSolver.cs", """
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
                        """),
                        ("Main.cs", """
                        internal partial class Program
                        {
                            static void Main(string[] args)
                            {
                                if (args.Length > 0)
                                {
                                    var a = args[0];
                                    if (a == "-h" || a == "--help")
                                    {
                                        System.Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                                        System.Console.WriteLine();
                                        System.Console.WriteLine(@"Options:
                        -i, --interactive   Run interactive mode.
                        -h, --help          Show this help.");
                                        return;
                                    }
                                    if (a == "-i" || a == "--interactive")
                                    {
                                        System.Console.WriteLine(@"Input class name");
                                        string line;
                                        do
                                        {
                                            line = System.Console.ReadLine().Trim();
                                        }
                                        while(line == "");
                                        a = line;
                                    }
                                    Run(a);
                                }
                                else
                                {
                                    Enumerate();
                                }
                            }
                            static partial void Run(string className);
                            static partial void Enumerate();
                        }
                        """),
    };
}