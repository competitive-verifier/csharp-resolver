using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Services;
using System;

namespace CompetitiveVerifierResolverTestLogger.Mtp;

public static class MtpHook
{
    /// <summary>
    /// Adds reporter to the Mtp entry point.
    /// </summary>
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] _) =>
        testApplicationBuilder.BuildReporter();

    private static void BuildReporter(this ITestApplicationBuilder testApplicationBuilder)
    {
        var compositeExtensionFactory = new CompositeExtensionFactory<ResolveContext>(serviceProvider =>
        {
            var commandLineOptions = serviceProvider.GetCommandLineOptions();
            return new ResolveContext(LoggerOptionsProvider.FromCommandLine(commandLineOptions));
        });

        testApplicationBuilder.TestHost.AddDataConsumer(compositeExtensionFactory);
        testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(compositeExtensionFactory);
        testApplicationBuilder.CommandLine.AddProvider(() => new LoggerOptionsProvider());
    }
}
