using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Services;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CompetitiveVerifierResolverTestLogger.Mtp;

internal class ResolveContext : IDataConsumer, ITestSessionLifetimeHandler
{
    public ResolveContext(LoggerOptionsDefault loggerOptions)
    {
        if (loggerOptions is not LoggerOptions options)
            return;

        var outDir = options.OutputDirectory.Replace('\\', '/');
        var fullPath = Path.GetFullPath(outDir).Replace('\\', '/');

        if (outDir != fullPath)
        {
            WriteWarning("OutDirectory parameter requires full path.");
        }

        OutputDirectory = fullPath;
    }
    public string? OutputDirectory { get; }
    public Task<bool> IsEnabledAsync() => Task.FromResult(OutputDirectory != null);

    public string Uid => "CompetitiveVerifierResolverTestLogger";
    public string DisplayName => Uid;
    public string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
    public string Description => "Save the test results";

    private TestResultWriter? _writer;

    public async Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext)
    {
        var testSuiteName =
            Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownTestSuite";
        var targetFrameworkName =
            AppContext.TargetFrameworkName ?? RuntimeInformation.FrameworkDescription;
        _writer = new(testSuiteName, targetFrameworkName);
    }
    public async Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext)
    {
        try
        {
            _writer?.WriteToCsv(OutputDirectory);
        }
        catch (Exception ex)
        {
            WriteError(ex.ToString());
        }
    }

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];
    public async Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        if (value is not TestNodeUpdateMessage message)
        {
            throw new InvalidOperationException($"Invalid message: {value.GetType().FullName}");
        }

        var className = message.TestNode.TryGetClassName();

        if (className is null)
        {
            return;
        }

        switch (message.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>())
        {
            case PassedTestNodeStateProperty:
                _writer?.Increment(className, Outcome.Success);
                break;
            case FailedTestNodeStateProperty:
            case ErrorTestNodeStateProperty:
            case TimeoutTestNodeStateProperty:
                _writer?.Increment(className, Outcome.Failure);
                break;
            case SkippedTestNodeStateProperty:
            case CancelledTestNodeStateProperty:
                _writer?.Increment(className, Outcome.Skipped);
                break;
        }
    }
}
