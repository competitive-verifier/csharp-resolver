using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompetitiveVerifierResolverTestLogger;

internal record TestResultWriter(string TestSuiteName, string TargetFrameworkName)
{
    private readonly Dictionary<string, TestResultCount> _results = [];

    public void Increment(string className, Outcome outcome)
    {
#if NET6_0_OR_GREATER
        ref var result = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(_results, className, out _);
        result.ClassName = className;
        result.Increment(outcome);
#else
        _results.TryGetValue(className, out TestResultCount result);
        result.ClassName = className;
        result.Increment(outcome);
        _results[className] = result;
#endif
    }

    public void WriteToCsv(string? directory)
    {
        string? filePath = null;

        if (directory is not null)
        {
            new DirectoryInfo(directory).Create();
            filePath = Path.Combine(directory, $"CompetitiveVerifier-{TestSuiteName}-{TargetFrameworkName}.csv");
        }

        var sw = filePath?.Pipe(s => new StreamWriter(new FileStream(s, FileMode.Create), new UTF8Encoding(false)));
        if (filePath is not null) Console.WriteLine($"CompetitiveVerifierResolverTestLogger: write to {filePath}");

        var resultsArray = _results.Values.ToArray();
        Array.Sort(resultsArray, (a, b) => string.Compare(a.ClassName, b.ClassName, StringComparison.Ordinal));

        using var tee = new TeeStream(sw);
        tee.WriteLine("Class,success,skipped,failure");
        foreach ((string className, int success, int skipped, int failure) in resultsArray)
        {
            tee.WriteLine($"{className},{success},{skipped},{failure}");
        }
    }
}