using CompetitiveVerifierCsResolver.Verifier;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.CommandLine;
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

        var unittestOption = new Option<FileInfo?>(
            aliases: new[] { "--unittest", "-u" },
            description: "Specify unittest result csv path.")
        {
        };
        var problemsOption = new Option<FileInfo?>(
            aliases: new[] { "--problems", "-p" },
            description: "Specify output of CompetitiveVerifierProblem.")
        {
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
            var unittest = ctx.ParseResult.GetValueForOption(unittestOption);
            var problems = ctx.ParseResult.GetValueForOption(problemsOption);
            var properties = ctx.ParseResult.GetValueForOption(propertiesOption);

            await Resolve(solutionPath, include, exclude, unittest, problems, properties, ctx.GetCancellationToken());
        });

        return await rootCommand.InvokeAsync(args);
    }

    public static async Task Resolve(
            string solutionPath,
            string[] include,
            string[] exclude,
            FileInfo? unittest = null,
            FileInfo? problems = null,
            ImmutableDictionary<string, string>? properties = null,
            CancellationToken cancellationToken = default
        )
    {
        var includeGlob = new GlobCollection(include.Select(s => Glob.Parse(s.Trim())));
        var excludeGlob = new GlobCollection(exclude.Select(s => Glob.Parse(s.Trim())));

        var matcher = new Matcher(includeGlob, excludeGlob);

        properties ??= ImmutableDictionary<string, string>.Empty;
        if (unittest is null && problems is null)
        {
            WriteWarning($"Both {nameof(unittest)} and {nameof(problems)} are null.");
            return;
        }

        Dictionary<string, UnitTestResult> testResults;
        if (unittest is null)
            testResults = new();
        else
        {
            using (var fs = unittest.OpenRead())
                testResults = ParseUnitTestResults(fs);
            if (testResults.Count == 0)
            {
                WriteWarning($"{nameof(unittest)} is empty.");
            }
        }


        Dictionary<string, ProblemVerification[]> problemVerifications;
        if (problems is null)
            problemVerifications = new();
        else
        {
            using (var fs = problems.OpenRead())
                problemVerifications = ParseProblemVerifications(fs) ?? new();
            if (problemVerifications.Count == 0)
            {
                WriteWarning($"{nameof(problems)} is empty.");
            }
        }

        var files = ImmutableDictionary.CreateBuilder<string, VerificationFile>();
        var workspace = MSBuildWorkspace.Create(properties);
        var solution = await workspace.OpenSolutionAsync(solutionPath, progress: new Progress(), cancellationToken: cancellationToken);

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

                var vf = new VerificationFile(dependencies, ListSpecialComments(tree.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)), verificationBuilder.ToImmutable());

                if (files.TryGetValue(relative, out var prev))
                    vf = vf.Merge(prev);
                files[relative] = vf;
            }
        }

        var result = new VerificationInput(files.ToImmutable());
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
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

    [GeneratedRegex(@"^\s*Class\s*,\s*success\s*,\s*skipped\s*,\s*failure\s*")]
    private static partial Regex UnitTestResultHeader();
    static Dictionary<string, UnitTestResult> ParseUnitTestResults(Stream stream)
    {
        var headerRegex = UnitTestResultHeader();
        using var sr = new StreamReader(stream);
        var firstLine = sr.ReadLine();
        if (firstLine == null) throw new ArgumentException("Failed to parse UnitTestResult csv.");

        var names = firstLine.Split(',');
        var d = new Dictionary<string, UnitTestResult>();

        while (sr.ReadLine() is string line)
        {
            if (headerRegex.IsMatch(line)) continue;
            var values = line.Split(',');
            if (values.Length == 0) continue;

            var b = new Builder(values[0]);
            for (int i = 1; i < values.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        b.Success = ParseLax(values[i]);
                        break;
                    case 2:
                        b.Skipped = ParseLax(values[i]);
                        break;
                    case 3:
                        b.Failure = ParseLax(values[i]);
                        break;
                }
            }
            var res = new UnitTestResult(b.Name, b.Success, b.Skipped, b.Failure);
            if (d.TryGetValue(b.Name, out var prev))
                res = res.Add(prev);
            d[b.Name] = res;
        }
        return d;
        static int ParseLax(string v)
        {
            _ = int.TryParse(v, out var result);
            return result;
        }
    }
    private class Builder
    {
        public Builder(string name)
        {
            Name = name;
        }
        public string Name;
        public int Success;
        public int Skipped;
        public int Failure;
    }
    static Dictionary<string, ProblemVerification[]>? ParseProblemVerifications(Stream stream)
    {
        return JsonSerializer.Deserialize<Dictionary<string, ProblemVerification[]>>(stream, new JsonSerializerOptions
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
        });
    }
    class Progress : IProgress<ProjectLoadProgress>
    {
        public void Report(ProjectLoadProgress p)
        {
            Console.Error.WriteLine($"Project:{p.FilePath} {(p.Operation == ProjectLoadOperation.Resolve ? $"({p.TargetFramework})" : "")} {p.Operation} {p.ElapsedTime.TotalMilliseconds:.}ms");
        }
    }

    static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Error.WriteLine($"Warning: {message}");
        Console.ResetColor();
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
        {
            Console.Error.WriteLine($"::warning ::{message}");
        }
    }
}