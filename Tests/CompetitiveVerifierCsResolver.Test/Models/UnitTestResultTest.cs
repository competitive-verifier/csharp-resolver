using CompetitiveVerifierCsResolver.Verifier;
using System.Text;
using System.Threading.Tasks;

namespace CompetitiveVerifierCsResolver.Models;

public class UnitTestResultTest
{
    public static readonly IEnumerable<(UnitTestResult, UnitTestResult, UnitTestResult)> Add_Data =
    [
        (new("Foo", 1, 2, 3), new("Foo", 10, 10, 10), new("Foo", 11, 12, 13)),
        (new("Foo", 1, 2, 3), new("Foo", 0, 0, 0), new("Foo", 1, 2, 3)),
    ];

    [Test]
    [MethodDataSource(nameof(Add_Data))]
    public async Task Add(UnitTestResult a, UnitTestResult b, UnitTestResult expected)
    {
        await Assert.That(a.Add(b)).IsEqualTo(expected);
    }

    public static readonly IEnumerable<(UnitTestResult, ConstVerificationStatus[])> EnumerateVerifications_Data =
    [
        (new("Foo", 1, 2, 3), [
            ConstVerificationStatus.Success,
            ConstVerificationStatus.Skipped,
            ConstVerificationStatus.Skipped,
            ConstVerificationStatus.Failure,
            ConstVerificationStatus.Failure,
            ConstVerificationStatus.Failure,
        ]),
    ];

    [Test]
    [MethodDataSource(nameof(EnumerateVerifications_Data))]
    public async Task EnumerateVerifications(UnitTestResult a, ConstVerificationStatus[] expected)
    {
        await Assert.That(a.EnumerateVerifications()).IsEquivalentTo(expected.Select(s => new ConstVerification(s)));
    }

    public class ParseResult : Dictionary<string, UnitTestResult>;
    public static readonly IEnumerable<(string, ParseResult)> Parse_Data =
    [
        ("""
            Class, success , skipped ,failure
            Foo,1,2,3
            Foo,10,10,10
            Bar,10,10,10
            """, new (){
                { "Foo", new("Foo", 11, 12, 13) },
                { "Bar", new("Bar", 10, 10, 10) },
            })
    ];

    [Test]
    [MethodDataSource(nameof(Parse_Data))]
    public async Task Parse(string input, Dictionary<string, UnitTestResult> expected)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
        await Assert.That(UnitTestResult.Parse(ms)).IsEquivalentTo(expected);
    }
}