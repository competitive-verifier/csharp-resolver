using CompetitiveVerifierCsResolver.Verifier;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CompetitiveVerifierCsResolver;
public partial class CsResolver
{
    public static async Task<int> RunAsync(string[] args)
    {
        var solutionArgument = new Argument<string>("solutionPath", "Specify solution path");

        var includeOption = new Option<string[]>(
            name: "--include",
            getDefaultValue: () => new[] { "**" },
            description: "Include glob patterns.")
        {
            AllowMultipleArgumentsPerToken = true,
        };
        var excludeOption = new Option<string[]>(
            name: "--exclude",
            getDefaultValue: () => new[] { "**/obj", "**/bin" },
            description: "Exclude glob patterns.")
        {
            AllowMultipleArgumentsPerToken = true,
        };

        var unittestOption = new Option<FileInfo[]?>(
            aliases: new[] { "--unittest", "-u" },
            description: "Specify unittest result csv paths.")
        {
            AllowMultipleArgumentsPerToken = true,
        };
        var problemsOption = new Option<FileInfo[]?>(
            aliases: new[] { "--problems", "-p" },
            description: "Specify outputs of CompetitiveVerifierProblem.")
        {
            AllowMultipleArgumentsPerToken = true,
        };
        var propertiesOption = new Option<ImmutableDictionary<string, string>?>(
            name: "properties",
            parseArgument: (res) => res.Tokens
            .SelectMany(t => t.Value.Split(';'))
            .Select(s =>
            {
                var sp = s.Split('=');
                return (sp[0], sp[1]);
            })
            .ToImmutableDictionary(t => t.Item1, t => t.Item2),
            description: "MSBuild properties separated by semicolon. e.g. WarningLevel=2;OutDir=bin\\Debug")
        {
        };

        var rootCommand = new RootCommand("C# resolver for competitive-verifier")
        {
            solutionArgument,
            includeOption,
            excludeOption,
            unittestOption,
            problemsOption,
            propertiesOption,
    };

        rootCommand.SetHandler(async ctx =>
        {
            var solutionPath = ctx.ParseResult.GetValueForArgument(solutionArgument)!;
            var include = ctx.ParseResult.GetValueForOption(includeOption)!;
            var exclude = ctx.ParseResult.GetValueForOption(excludeOption)!;
            var unittest = ctx.ParseResult.GetValueForOption(unittestOption) ?? Array.Empty<FileInfo>();
            var problems = ctx.ParseResult.GetValueForOption(problemsOption) ?? Array.Empty<FileInfo>();
            var properties = ctx.ParseResult.GetValueForOption(propertiesOption);

            await new CsResolver(ctx.Console).Resolve(
                solutionPath,
                include,
                exclude,
                unittest.ToImmutableArray(),
                problems.ToImmutableArray(),
                properties ?? ImmutableDictionary<string, string>.Empty,
                ctx.GetCancellationToken());
        });

        return await rootCommand.InvokeAsync(args);
    }

    private readonly IConsole console;
    public CsResolver(IConsole console)
    {
        this.console = console;
    }

    public async Task Resolve(
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

        var includeGlob = new GlobCollection(include.Select(s => Glob.Parse(s.Trim())));
        var excludeGlob = new GlobCollection(exclude.Select(s => Glob.Parse(s.Trim())));

        var matcher = new Matcher(includeGlob, excludeGlob);

        properties ??= ImmutableDictionary<string, string>.Empty;
        if (unittest.IsDefaultOrEmpty && problems.IsDefaultOrEmpty)
        {
            WriteWarning($"Both {nameof(unittest)} and {nameof(problems)} are empty.");
            return;
        }

        Dictionary<string, UnitTestResult> testResults = new();
        if (!unittest.IsDefaultOrEmpty)
        {
            foreach (var p in unittest)
            {
                using var fs = p.OpenRead();
                testResults.Add(Parse.ParseUnitTestResults(fs));
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
                if (Parse.ParseProblemVerifications(fs) is { } dd)
                    problemVerifications.Add(dd);
            }
            if (problemVerifications.Count == 0)
            {
                WriteWarning($"{nameof(problems)} is empty.");
            }
        }

        var files = ImmutableDictionary.CreateBuilder<string, VerificationFile>();
        var workspace = MSBuildWorkspace.Create(properties);
        var solution = await workspace.OpenSolutionAsync(solutionPath, progress: new Progress(console), cancellationToken: cancellationToken);

        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation is null) continue;

            foreach (var tree in compilation.SyntaxTrees)
            {
                var path = tree.FilePath;
                if (matcher.RelativePath(path) is not string relative) continue;

                var semanticModel = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                var finder = new TypeFinder(semanticModel, cancellationToken);
                finder.Visit(await tree.GetRootAsync(cancellationToken));

                var dependencies = finder.UsedFiles.Select(matcher.RelativePath).OfType<string>().ToImmutableHashSet();
                var verificationBuilder = ImmutableArray.CreateBuilder<Verification>();
                foreach (var typeName in finder.DefinedTypeNames)
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

                var attrs = ListSpecialComments(tree.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                if (attrs.GetValueOrDefault("UNITTEST") is string unittestEnv)
                {
                    WriteWarning($"{relative}: competitive-verifier-cs-resolver doesn't support UNITTEST attribute. Use --unittest option.");
                }

                var vf = new VerificationFile(dependencies, attrs, verificationBuilder.ToImmutable());

                if (files.TryGetValue(relative, out var prev))
                    vf = vf.Merge(prev);
                files[relative] = vf;
            }
        }

        var result = new VerificationInput(files.ToImmutable());
        console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
        }));
    }

    [GeneratedRegex(@"\b(?:competitive-verifier):\s*(\S+)(?:\s(.*))?$")]
    private static partial Regex ListSpecialCommentsRegex();
    static ImmutableDictionary<string, string> ListSpecialComments(string[] lines)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        var regex = ListSpecialCommentsRegex();
        foreach (var line in lines)
        {
            var m = regex.Match(line);
            if (m.Success)
            {
                builder[m.Groups[1].Value] = m.Groups[2].Value;
            }
        }

        return builder.ToImmutable();
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