using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompetitiveVerifierResolverTestLogger;
internal record ResolveContext(string? OutDirectory)
{
    private readonly object _lock = new();
    private readonly List<TestResult> _testResults = new();
    private TestRunCriteria? _testRunCriteria;

    internal void OnTestRunStart(TestRunStartEventArgs e)
    {
        _testRunCriteria = e.TestRunCriteria;
    }

    public void OnTestResult(TestResultEventArgs e)
    {
        lock (_lock)
        {
            _testResults.Add(e.Result);
        }
    }

    public void OnTestRunComplete(TestRunCompleteEventArgs _)
    {
        lock (_lock)
        {
            try
            {
                var testSuiteName =
                    _testRunCriteria?.Sources.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension) ??
                    "UnknownTestSuite";

                var targetFrameworkName =
                    _testRunCriteria?.TryGetTargetFramework() ??
                    "UnknownTargetFramework";

                OutDirectory?.Pipe(s => new DirectoryInfo(s))?.Create();
                var filePath = OutDirectory?.Pipe(s => Path.Combine(s, $"CompetitiveVerifier-{testSuiteName}-{targetFrameworkName}.csv"));

                var sw = filePath?.Pipe(s => new StreamWriter(new FileStream(s, FileMode.Create), new UTF8Encoding(false)));
                if (filePath is not null) Console.WriteLine($"CompetitiveVerifierResolverTestLogger: write to {filePath}.");

                using var tee = new TeeStream(sw);
                tee.WriteLine("Class,success,skipped,failure");
                foreach (var gr in _testResults.GroupBy(r => GetClassNameFromFullyQualifiedName(r.TestCase.FullyQualifiedName), r => r.Outcome))
                {
                    (int success, int skipped, int failure) = OutcomeCount(gr);
                    tee.WriteLine($"{gr.Key},{success},{skipped},{failure}");
                }
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
            }
        }
    }

    static string GetClassNameFromFullyQualifiedName(string fullyQualifiedName)
    {
        var classAndMethodLength = Math.Min((uint)fullyQualifiedName.IndexOf('('), (uint)fullyQualifiedName.Length);
        var classNameLength = Math.Min((uint)fullyQualifiedName.LastIndexOf('.', (int)classAndMethodLength - 1), classAndMethodLength);

#pragma warning disable IDE0057
        return fullyQualifiedName.Substring(0, (int)classNameLength);
#pragma warning restore IDE0057
    }

    (int success, int skipped, int failure) OutcomeCount(IEnumerable<TestOutcome> outcomes)
    {
        int success = 0;
        int skipped = 0;
        int failure = 0;
        foreach (var c in outcomes)
        {
            switch (c)
            {
                case TestOutcome.Failed:
                    ++failure;
                    break;
                case TestOutcome.Passed:
                    ++success;
                    break;
                case TestOutcome.Skipped:
                    ++skipped;
                    break;
                case TestOutcome.None:
                case TestOutcome.NotFound:
                    break;
            }
        }
        return (success, skipped, failure);
    }
}
