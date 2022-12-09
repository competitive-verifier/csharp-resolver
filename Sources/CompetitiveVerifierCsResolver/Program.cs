using CompetitiveVerifierCsResolver;
using Microsoft.Build.Locator;
using System.Collections.Immutable;
using System.CommandLine;
using System.Runtime.Loader;

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
        name: "--properties",
        parseArgument: (res) => res.Tokens
        .SelectMany(t => t.Value.Split(';'))
        .Select(s =>
        {
            var sp = s.Split('=');
            return (sp[0].Trim(), sp[1].Trim());
        })
        .ToImmutableDictionary(t => t.Item1, t => t.Item2),
        description: "MSBuild properties separated by semicolon. e.g. WarningLevel=2;Configuration=Release")
    {
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

    rootCommand.SetHandler(async ctx =>
    {
        var solutionPath = ctx.ParseResult.GetValueForArgument(solutionArgument)!;
        var include = ctx.ParseResult.GetValueForOption(includeOption)!;
        var exclude = ctx.ParseResult.GetValueForOption(excludeOption)!;
        var unittest = ctx.ParseResult.GetValueForOption(unittestOption) ?? Array.Empty<FileInfo>();
        var problems = ctx.ParseResult.GetValueForOption(problemsOption) ?? Array.Empty<FileInfo>();
        var properties = ctx.ParseResult.GetValueForOption(propertiesOption);

        await new CsResolver(ctx.Console).ResolveAsync(
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