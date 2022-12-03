using CompetitiveVerifierCsResolver.Verifier;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CompetitiveVerifierCsResolver;
internal static partial class Parse
{
    [GeneratedRegex(@"^\s*Class\s*,\s*success\s*,\s*skipped\s*,\s*failure\s*")]
    private static partial Regex UnitTestResultHeader();
    public static Dictionary<string, UnitTestResult> ParseUnitTestResults(Stream stream)
    {
        var headerRegex = UnitTestResultHeader();
        using var sr = new StreamReader(stream);
        var firstLine = sr.ReadLine();
        if (firstLine == null) throw new ArgumentException("Failed to parse UnitTestResult csv.");

        var names = firstLine.Split(',');
        var d = new Dictionary<string, UnitTestResult>();

        while (sr.ReadLine() is string line)
        {
            if (headerRegex.IsMatch(line)) continue;
            var values = line.Split(',');
            if (values.Length == 0) continue;

            var b = new Builder(values[0]);
            for (int i = 1; i < values.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        b.Success = ParseLax(values[i]);
                        break;
                    case 2:
                        b.Skipped = ParseLax(values[i]);
                        break;
                    case 3:
                        b.Failure = ParseLax(values[i]);
                        break;
                }
            }
            var res = new UnitTestResult(b.Name, b.Success, b.Skipped, b.Failure);
            if (d.TryGetValue(b.Name, out var prev))
                res = res.Add(prev);
            d[b.Name] = res;
        }
        return d;
        static int ParseLax(string v)
        {
            _ = int.TryParse(v, out var result);
            return result;
        }
    }
    private class Builder
    {
        public Builder(string name)
        {
            Name = name;
        }
        public string Name;
        public int Success;
        public int Skipped;
        public int Failure;
    }
    public static Dictionary<string, ProblemVerification[]>? ParseProblemVerifications(Stream stream)
    {
        return JsonSerializer.Deserialize<Dictionary<string, ProblemVerification[]>>(stream, new JsonSerializerOptions
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
        });
    }
}
