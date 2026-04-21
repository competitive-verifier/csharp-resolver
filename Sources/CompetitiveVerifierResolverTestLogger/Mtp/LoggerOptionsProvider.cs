using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitiveVerifierResolverTestLogger.Mtp;

public record LoggerOptionsDefault
{
    public virtual bool IsEnabled => false;
}
public record LoggerOptions(
    string OutputDirectory
) : LoggerOptionsDefault
{
    public override bool IsEnabled => true;
}
internal class LoggerOptionsProvider : ICommandLineOptionsProvider
{
    private static readonly CommandLineOption ReportOption = new(
        $"report-competitive-verifier",
        "Specify the directory where the test results should be saved",
        ArgumentArity.ExactlyOne,
        false
    );

    public string Uid => "CompetitiveVerifierResolverTestLogger/Options";
    public string DisplayName => Uid;
    public string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
    public string Description => "Provide the options of CompetitiveVerifierResolverTestLogger";

    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() => [ReportOption];

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task<ValidationResult> ValidateCommandLineOptionsAsync(ICommandLineOptions commandLineOptions)
        => ValidationResult.ValidTask;

    public Task<ValidationResult> ValidateOptionArgumentsAsync(CommandLineOption commandOption, string[] arguments)
        => ValidationResult.ValidTask;

    public static LoggerOptionsDefault FromCommandLine(ICommandLineOptions commandLineOptions)
    {
        if (!commandLineOptions.TryGetOptionArgumentList(ReportOption.Name, out var arguments))
        {
#if DEBUG
            var dir = System.IO.Path.GetFullPath($"./VerificationCsv");
            return new LoggerOptions(dir);
#else
            return new LoggerOptionsDefault();
#endif
        }
        return new LoggerOptions(arguments[0]);
    }
}
