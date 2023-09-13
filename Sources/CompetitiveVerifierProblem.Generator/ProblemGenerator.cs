using CompetitiveVerifierProblem.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;
using System.Text;
using System.Collections;

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
                    return context.SemanticModel.GetDeclaredSymbol(syntax, token);
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
        static ((string FullName, IEnumerable<string> Names)? Names, IEnumerable<Diagnostic> Diagnostics) GetNames(INamedTypeSymbol s)
        {
            if (s.IsAbstract) return (null, Array.Empty<Diagnostic>());

            var names = new List<string>();
            var fullName = s.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
            names.Add(fullName);

            if (!s.Constructors.Select(c => c.Parameters.Length).Contains(0))
            {
                return (
                            null,
                            s.DeclaringSyntaxReferences
                             .Select(r => Location.Create(r.SyntaxTree, r.Span))
                             .Select(loc => DiagnosticDescriptors.VERIFY0002_WithoutDefaultConstructor(fullName, loc))
                       );
            }

            return ((fullName, new HashSet<string> { fullName, s.Name }), Array.Empty<Diagnostic>());
        }

        var fullNames = new HashSet<string>();
        var namesDic = new Dictionary<string, List<string>>(); // Key: name, Value: FullName

        var classesCallToJson = new StringBuilder();
        var solverSelector = new StringBuilder();
        foreach (var (namesTup, diags) in classes.Select(GetNames).OrderBy(t => t.Names?.FullName, StringComparer.Ordinal))
        {
            foreach (var diag in diags)
                context.ReportDiagnostic(diag);
            if (namesTup is (string fullName, IEnumerable<string> names))
            {
                foreach (var name in names)
                {
                    if (!namesDic.TryGetValue(name, out var lst))
                        namesDic[name] = lst = new List<string>();
                    lst.Add(fullName);
                }

                Debug.Assert(!fullNames.Contains(fullName));
                fullNames.Add(fullName);
            }
        }

        foreach (var fullName in fullNames)
        {
            namesDic.Remove(fullName);
            classesCallToJson.AppendLine($"new {fullName}(),");
            solverSelector.Append("case ").Append(Literal(fullName)).Append(":return new ").Append(fullName).AppendLine("();");
        }

        foreach (var (name, full) in namesDic)
        {
            Debug.Assert(full.Count > 0);
            if (fullNames.Contains(name))
            {
                Debug.Assert(fullNames.IsSupersetOf(full));
            }
            else if (full.Count == 1)
            {
                var fullName = full[0];
                solverSelector.Append("case ").Append(Literal(name)).Append(":return new ").Append(fullName).AppendLine("();");
            }
            else
            {
                solverSelector
                    .Append("case ").Append(Literal(name)).Append($":throw new System.ArgumentException(\"")
                    .Append(name).Append(" is ambiguous");

                full.Sort(StringComparer.Ordinal);
                foreach (var fullName in full)
                {
                    solverSelector.Append(", ").Append(fullName);
                }
                solverSelector.AppendLine(".\", nameof(className));");
            }
        }

        context.AddSource("Main.impl.cs", $$$"""
            #pragma warning disable IDE0161,CS8602
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
                        default: throw new System.ArgumentException($"{className} is not found.", nameof(className));
                    }
                }
            }
            """);
    }
}