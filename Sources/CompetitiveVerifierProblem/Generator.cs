﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CompetitiveVerifierProblem;

[Generator]
public partial class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialization);

        var baseSolver = context.CompilationProvider
            .Select(static (compilation, token) =>
            {
                token.ThrowIfCancellationRequested();
                return compilation.GetTypeByMetadataName("CompetitiveVerifier.ProblemSolver");
            })
            .WithComparer(SymbolEqualityComparer.Default);

        var classess = context.SyntaxProvider
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
                var builder = ImmutableArray.CreateBuilder<string>();
                foreach (var symbol in decs)
                    if (symbol is not null && SymbolEqualityComparer.Default.Equals(baseSolver, symbol.BaseType))
                    {
                        builder.Add(symbol.ToDisplayString());
                    }
                return builder.ToImmutable();
            });


        context.RegisterImplementationSourceOutput(classess, ImplementationSource);
    }

    private void ImplementationSource(SourceProductionContext context, ImmutableArray<string> classes)
    {
        var runSelector = new StringBuilder();

        var classesCallToJson = "new CompetitiveVerifier.ProblemSolver[]{" + string.Join(",", classes.Select(c => $"new {c}()")) + "}";
        foreach (var c in classes)
        {
            runSelector.Append("case \"").Append(c).Append("\":solver = new ").Append(c).AppendLine("();break;");
        }
        context.AddSource("Main.impl.cs", $$$"""
            internal partial class Program
            {
                static partial void Enumerate()
                {
                    var classes = {{{classesCallToJson}}};

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
                        System.Console.Write(c.ToJson());
                    }
                    System.Console.WriteLine('}');
                }

                static partial void Run(string className)
                {
                    CompetitiveVerifier.ProblemSolver solver;
                    switch(className)
                    {
                        {{{runSelector}}}
                        default: throw new System.ArgumentException($"{className} is not found.", nameof(className));
                    }
                    solver.Solve();
                }
            }
            """);
    }

    private void PostInitialization(IncrementalGeneratorPostInitializationContext context)
    {
        var token = context.CancellationToken;
        token.ThrowIfCancellationRequested();

        context.AddSource("ProblemSolver.cs", """
            
            #nullable disable
            namespace CompetitiveVerifier
            {
                using System.IO;
                using System.Runtime.Serialization;
                using System.Runtime.Serialization.Json;
                using System.Text;

                internal abstract class ProblemSolver
                {
                    public abstract string Url { get; }
                    public virtual double? Error => null;

                    public abstract void Solve();
                    public string ToJson()
                    {
                        using (var stream = new MemoryStream())
                        using (var sw = new StringWriter())
                        {
                            var serializer = new DataContractJsonSerializer(typeof(JsonDataContract));
                            serializer.WriteObject(stream, new JsonDataContract(this));
                            var json = Encoding.UTF8.GetString(stream.ToArray());
                            sw.Write(json);
                            return sw.ToString();
                        }
                    }
                    [DataContract]
                    private struct JsonDataContract
                    {
                        [DataMember(Name = "type", IsRequired = true)]
                        public string Type { set; get; } = "problem";
                        [DataMember(Name = "problem", IsRequired = true)]
                        public string Url { set; get; }
                        [DataMember(Name = "command", IsRequired = true)]
                        public string Command { set; get; }
                        [DataMember(Name = "error", EmitDefaultValue = false)]
                        public double? Error { set; get; }

                        public JsonDataContract(ProblemSolver solver)
                        {
                            Type = "problem";
                            Url = solver.Url;
                            Command = $"dotnet {System.Reflection.Assembly.GetEntryAssembly().Location} {solver.GetType().FullName}";
                            Error = solver.Error;
                        }
                    }
                }
            }
            """);

        context.AddSource("Main.cs", """
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
            """);
    }
}