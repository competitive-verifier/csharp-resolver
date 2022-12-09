using CompetitiveVerifierCsResolver.Models;
using CompetitiveVerifierCsResolver.Resolve;
using CompetitiveVerifierCsResolver.Verifier;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.IO;
using System.Text.RegularExpressions;

namespace CompetitiveVerifierCsResolver;
public partial class CsResolver
{
    private readonly IConsole console;
    public CsResolver(IConsole console)
    {
        this.console = console;
    }

    public async Task ResolveAsync(
            string solutionPath,
            string[] include,
            string[] exclude,
            ImmutableArray<FileInfo> unittest,
            ImmutableArray<FileInfo> problems,
            ImmutableDictionary<string, string> properties,
            CancellationToken cancellationToken = default
        )
    {
        WriteDebug("Arguments");
        WriteDebug($"solutionPath={solutionPath}");
        WriteDebug($"include={string.Join(",", include)}");
        WriteDebug($"exclude={string.Join(",", exclude)}");
        WriteDebug($"unittest={string.Join(",", unittest.Select(f => f.FullName))}");
        WriteDebug($"problems={string.Join(",", problems.Select(f => f.FullName))}");
        WriteDebug($"MS build properties={string.Join(' ', properties.Select(p => $"{p.Key}={p.Value}"))}");


        properties ??= ImmutableDictionary<string, string>.Empty;
        if (unittest.IsDefaultOrEmpty && problems.IsDefaultOrEmpty)
        {
            WriteWarning($"Both {nameof(unittest)} and {nameof(problems)} are empty.");
        }

        Dictionary<string, UnitTestResult> testResults = new();
        if (!unittest.IsDefaultOrEmpty)
        {
            foreach (var p in unittest)
            {
                using var fs = p.OpenRead();
                testResults.Add(UnitTestResult.Parse(fs));
            }
            if (testResults.Count == 0)
            {
                WriteWarning($"{nameof(unittest)} is empty.");
            }
        }


        Dictionary<string, ProblemVerification[]> problemVerifications = new();
        if (!problems.IsDefaultOrEmpty)
        {
            foreach (var p in problems)
            {
                using var fs = p.OpenRead();
                if (ProblemVerification.Parse(fs) is { } dd)
                    problemVerifications.Add(dd);
            }
            if (problemVerifications.Count == 0)
            {
                WriteWarning($"{nameof(problems)} is empty.");
            }
        }

        var workspace = MSBuildWorkspace.Create(properties);
        var solution = await workspace.OpenSolutionAsync(solutionPath, progress: new Progress(console), cancellationToken: cancellationToken);

        var includeGlob = new GlobCollection(include.Select(s => Glob.Parse(s.Trim())));
        var excludeGlob = new GlobCollection(exclude.Select(s => Glob.Parse(s.Trim())));

        var result = await ResolveImplAsync(solution, new PathResolver(Environment.CurrentDirectory, includeGlob, excludeGlob), testResults, problemVerifications, cancellationToken);
        console.WriteLine(result.ToJson());
    }
    internal async Task<VerificationInput> ResolveImplAsync(
            Solution solution,
            IPathResolver pathResolver,
            IDictionary<string, UnitTestResult> testResults,
            IDictionary<string, ProblemVerification[]> problemVerifications,
            CancellationToken cancellationToken = default
        )
    {
        var files = ImmutableSortedDictionary.CreateBuilder<string, VerificationFile>();
        var types = ImmutableSortedDictionary.CreateBuilder<string, ImmutableHashSet<string>>();

        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation is null) continue;

            foreach (var tree in compilation.SyntaxTrees)
            {
                var path = tree.FilePath;
                if (pathResolver.RelativePath(path) is not string relative) continue;

                var root = await tree.GetRootAsync(cancellationToken);
                var semanticModel = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                var typeFinder = new TypeFinder(semanticModel, cancellationToken);
                typeFinder.Visit(root);

                var dependencies = typeFinder.UsedFiles.Select(pathResolver.RelativePath).OfType<string>().ToImmutableHashSet();

                var treeString = tree.ToString();
                var treeLines = treeString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var attrBuilder = ImmutableSortedDictionary.CreateBuilder<string, object>();
                ListSpecialComments(attrBuilder, treeLines);

                if (attrBuilder.GetValueOrDefault("UNITTEST") is string unittestEnv)
                {
                    WriteWarning($"{relative}: competitive-verifier-cs-resolver doesn't support UNITTEST attribute. Use --unittest option.");
                }

                var urlsFinder = new UrlFinder(cancellationToken);
                urlsFinder.Visit(root);
                if (!urlsFinder.Urls.IsEmpty)
                    attrBuilder["links"] = urlsFinder.Urls;

                var vf = new VerificationFile(dependencies, attrBuilder.ToImmutable(), ImmutableArray<Verification>.Empty);

                if (files.TryGetValue(relative, out var prev))
                    vf = vf.Merge(prev);
                files[relative] = vf;
                types[relative] = types.GetValueOrDefault(relative, ImmutableHashSet<string>.Empty).Union(typeFinder.DefinedTypeNames);
            }
        }


        foreach (var (relative, typeNames) in types)
        {
            var verificationBuilder = ImmutableArray.CreateBuilder<Verification>();
            foreach (var typeName in typeNames)
            {
                if (testResults.TryGetValue(typeName, out var unitTestResult))
                {
                    verificationBuilder.AddRange(unitTestResult.EnumerateVerifications());
                }
                if (problemVerifications.TryGetValue(typeName, out var vs))
                {
                    verificationBuilder.AddRange(vs);
                }
            }
            if (files.TryGetValue(relative, out var vf))
            {
                files[relative] = vf with { Verification = verificationBuilder.ToImmutable() };
            }
        }

        return new(files.ToImmutable());
    }


    [GeneratedRegex(@"\b(?:competitive-verifier):\s*(\S+)(?:\s(.*))?$")]
    private static partial Regex ListSpecialCommentsRegex();
    static void ListSpecialComments(ImmutableSortedDictionary<string, object>.Builder builder, string[] lines)
    {
        var regex = ListSpecialCommentsRegex();
        foreach (var line in lines)
        {
            var m = regex.Match(line);
            if (m.Success)
            {
                builder[m.Groups[1].Value] = m.Groups[2].Value;
            }
        }
    }

    record class Progress(IConsole Console) : IProgress<ProjectLoadProgress>
    {
        public void Report(ProjectLoadProgress p)
        {
            Console.Error.WriteLine($"Project:{p.FilePath} {(p.Operation == ProjectLoadOperation.Resolve ? $"({p.TargetFramework})" : "")} {p.Operation} {p.ElapsedTime.TotalMilliseconds:.}ms");
        }
    }

    void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        console.Error.WriteLine($"Warning: {message}");
        Console.ResetColor();
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
        {
            console.Error.WriteLine($"::warning ::{message}");
        }
    }
    void WriteDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"{message}");
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
        {
            console.Error.WriteLine($"::debug ::{message}");
        }
    }
}

internal static class MergeExtension
{
    public static Dictionary<string, UnitTestResult> Add(this Dictionary<string, UnitTestResult> orig, Dictionary<string, UnitTestResult> other)
    {
        foreach (var (k, v) in other)
        {
            if (orig.TryGetValue(k, out var prev))
                orig[k] = prev.Add(v);
            else
                orig[k] = v;
        }
        return orig;
    }
    public static Dictionary<string, ProblemVerification[]> Add(this Dictionary<string, ProblemVerification[]> orig, Dictionary<string, ProblemVerification[]> other)
    {
        foreach (var (k, v) in other)
        {
            if (orig.TryGetValue(k, out var prev))
                orig[k] = prev.Concat(v).ToArray();
            else
                orig[k] = v;
        }
        return orig;
    }
}