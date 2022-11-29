using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace CompetitiveVerifierResolverTestLogger;

[FriendlyName("CompetitiveVerifier")]
[ExtensionUri("logger://competitive-verifier/resolver/v1")]
public class TestLogger : ITestLoggerWithParameters
{
    private ResolveContext? _context;
    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        Initialize(events, new Dictionary<string, string?> { });
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters)
    {
        if (parameters.TryGetValue("OutFile", out var outFile) && outFile is not null)
        {
            outFile = outFile.Replace('\\', '/');
            var fullPath = Path.GetFullPath(outFile).Replace('\\', '/');

            if (outFile != fullPath)
            {
                WriteWarning("OutFile parameter requires full path.");
            }
        }
        else
        {
            WriteWarning("specify OutFile. e.g. dotnet test --logger \"CompetitiveVerifier;OutFile=$PWD/out.csv\"");
        }

        _context = new ResolveContext(outFile);
        events.TestResult += (_, e) => _context.OnTestResult(e);
        events.TestRunComplete += (_, e) => _context.OnTestRunComplete(e);
    }

    static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Warning: {message}");
        Console.ResetColor();
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
        {
            Console.WriteLine($"::warning ::{message}");
        }
    }
}
