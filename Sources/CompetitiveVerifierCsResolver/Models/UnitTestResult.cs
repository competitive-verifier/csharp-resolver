using CompetitiveVerifierCsResolver.Verifier;
using System.Text.RegularExpressions;

namespace CompetitiveVerifierCsResolver.Models;
public partial record UnitTestResult(string Name, int Success, int Skipped, int Failure)
{
    public UnitTestResult Add(UnitTestResult other)
        => this with
        {
            Success = Success + other.Success,
            Skipped = Skipped + other.Skipped,
            Failure = Failure + other.Failure,
        };

    public IEnumerable<ConstVerification> EnumerateVerifications()
        => Enumerable.Repeat(new ConstVerification(ConstVerificationStatus.Success), Success)
        .Concat(Enumerable.Repeat(new ConstVerification(ConstVerificationStatus.Skipped), Skipped))
        .Concat(Enumerable.Repeat(new ConstVerification(ConstVerificationStatus.Failure), Failure));


    [GeneratedRegex(@"^\s*Class\s*,\s*success\s*,\s*skipped\s*,\s*failure\s*")]
    private static partial Regex UnitTestResultHeader();
    public static Dictionary<string, UnitTestResult> Parse(Stream stream)
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
}
