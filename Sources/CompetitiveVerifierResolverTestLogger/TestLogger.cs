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
        if (parameters.TryGetValue("OutDirectory", out var outDir) && outDir is not null)
        {
            outDir = outDir.Replace('\\', '/');
            var fullPath = Path.GetFullPath(outDir).Replace('\\', '/');

            if (outDir != fullPath)
            {
                WriteWarning("OutDirectory parameter requires full path.");
            }
        }
        else
        {
            WriteWarning("specify OutDirectory. e.g. dotnet test --logger \"CompetitiveVerifier;OutDirectory=$PWD/VerifierUnitTest\"");
        }

        _context = new ResolveContext(outDir);
        events.TestRunStart += (_, e) => _context.OnTestRunStart(e);
        events.TestResult += (_, e) => _context.OnTestResult(e);
        events.TestRunComplete += (_, e) => _context.OnTestRunComplete(e);
    }
}
