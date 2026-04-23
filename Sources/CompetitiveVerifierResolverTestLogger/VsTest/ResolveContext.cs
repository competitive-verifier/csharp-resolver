using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CompetitiveVerifierResolverTestLogger.VsTest;

internal class ResolveContext(string? OutDirectory)
{
    private readonly object _lock = new();
    private TestRunCriteria? _testRunCriteria;
    private TestResultWriter? _writer;

    internal void OnTestRunStart(TestRunStartEventArgs e)
    {
        _testRunCriteria = e.TestRunCriteria;

        var testSuiteName =
            _testRunCriteria?.Sources?.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension) ??
            "UnknownTestSuite";

        var targetFrameworkName =
            _testRunCriteria?.TryGetTargetFramework() ??
            "UnknownTargetFramework";
        _writer = new(testSuiteName, targetFrameworkName);
    }

    public void OnTestResult(TestResultEventArgs e)
    {
        lock (_lock)
        {
            var r = e.Result;
            var className = GetClassNameFromFullyQualifiedName(r.TestCase.FullyQualifiedName);

            switch (r.Outcome)
            {
                case TestOutcome.Failed:
                    _writer?.Increment(className, Outcome.Failure);
                    break;
                case TestOutcome.Passed:
                    _writer?.Increment(className, Outcome.Success);
                    break;
                case TestOutcome.Skipped:
                    _writer?.Increment(className, Outcome.Skipped);
                    break;
                case TestOutcome.None:
                case TestOutcome.NotFound:
                    break;
            }
        }
    }

    public void OnTestRunComplete(TestRunCompleteEventArgs _)
    {
        lock (_lock)
        {
            try
            {
                _writer?.WriteToCsv(OutDirectory);
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
}

internal static class TestRunCriteriaExtensions
{
    public static string? TryGetTargetFramework(this TestRunCriteria testRunCriteria)
    {
        if (string.IsNullOrWhiteSpace(testRunCriteria.TestRunSettings))
            return null;

        return (string?)XElement
            .Parse(testRunCriteria.TestRunSettings)
            .Element("RunConfiguration")?
            .Element("TargetFrameworkVersion");
    }
}
