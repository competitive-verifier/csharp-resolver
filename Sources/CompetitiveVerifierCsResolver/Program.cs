using CompetitiveVerifierCsResolver;
using Microsoft.Build.Locator;
using System.Collections.Immutable;
using System.CommandLine;
using System.Runtime.Loader;
using System.Text.Json;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

var instance = MSBuildLocator.RegisterDefaults();
AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, assemblyName) =>
{
    var path = Path.Combine(instance.MSBuildPath, assemblyName.Name + ".dll");
    if (File.Exists(path))
    {
        return assemblyLoadContext.LoadFromAssemblyPath(path);
    }

    return null;
};
return await RunAsync(args);


static async Task<int> RunAsync(string[] args)
{
    var solutionArgument = new Argument<string>("solutionPath")
    {
        Description = "Specify solution path",
    };

    var includeOption = new Option<string[]>("--include")
    {
        DefaultValueFactory = _ => ["**"],
        Description = "Include glob patterns.",
        AllowMultipleArgumentsPerToken = true,
    };
    var excludeOption = new Option<string[]>("--exclude")
    {
        DefaultValueFactory = _ => ["**/obj", "**/bin"],
        Description = "Exclude glob patterns.",
        AllowMultipleArgumentsPerToken = true,
    };

    var unittestOption = new Option<FileInfo[]?>("--unittest", "-u")
    {
        Description = "Specify unittest result csv paths.",
        AllowMultipleArgumentsPerToken = true,
    };
    var problemsOption = new Option<FileInfo[]?>("--problems", "-p")
    {
        Description = "Specify outputs of CompetitiveVerifierProblem.",
        AllowMultipleArgumentsPerToken = true,
    };
    var propertiesOption = new Option<ImmutableDictionary<string, string>?>("--properties")
    {
        CustomParser = (res) => res.Tokens
        .SelectMany(t => t.Value.Split(';'))
        .Select(s =>
        {
            var ss = s.AsSpan().Trim();
            var ix = ss.IndexOf('=');

            var key = ss[..ix].ToString();
            var val = ss[(ix + 1)..].ToString();

            return (key, val);
        })
        .ToImmutableDictionary(t => t.Item1, t => t.Item2),
        Description = "MSBuild properties separated by semicolon. e.g. WarningLevel=2;Configuration=Release",
        AllowMultipleArgumentsPerToken = true,
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


    rootCommand.SetAction(RunImpl);

    return await rootCommand.Parse(args).InvokeAsync();

    async Task RunImpl(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var solutionPath = parseResult.GetRequiredValue(solutionArgument)!;
        var include = parseResult.GetValue(includeOption)!;
        var exclude = parseResult.GetValue(excludeOption)!;
        var unittest = parseResult.GetRequiredValue(unittestOption) ?? [];
        var problems = parseResult.GetRequiredValue(problemsOption) ?? [];
        var properties = parseResult.GetValue(propertiesOption);

        await new CsResolver(
            parseResult.InvocationConfiguration.Output,
            parseResult.InvocationConfiguration.Error).ResolveAsync(
            solutionPath,
            include,
            exclude,
#pragma warning disable IDE0305
            unittest.ToImmutableArray(),
            problems.ToImmutableArray(),
#pragma warning restore IDE0305
            properties ?? ImmutableDictionary<string, string>.Empty,
            cancellationToken);
    }
}
