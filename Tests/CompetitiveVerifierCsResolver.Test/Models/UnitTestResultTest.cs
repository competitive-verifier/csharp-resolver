using CompetitiveVerifierCsResolver.Verifier;
using System.Text;

namespace CompetitiveVerifierCsResolver.Models;
public class UnitTestResultTest
{
    public static readonly TheoryData<UnitTestResult, UnitTestResult, UnitTestResult> Add_Data = new()
    {
        { new("Foo", 1, 2, 3), new("Foo", 10, 10, 10),new("Foo", 11, 12, 13) },
        { new("Foo", 1, 2, 3), new("Foo", 0, 0, 0),new("Foo", 1, 2, 3) },
    };

    [Theory]
    [MemberData(nameof(Add_Data))]
    public void Add(UnitTestResult a, UnitTestResult b, UnitTestResult expected)
    {
        Assert.Equal(expected, a.Add(b));
    }

    public static readonly TheoryData<UnitTestResult, ConstVerification[]> EnumerateVerifications_Data = new()
    {
        { new("Foo", 1, 2, 3), new ConstVerification[]{
            new(ConstVerificationStatus.Success),
            new(ConstVerificationStatus.Skipped),
            new(ConstVerificationStatus.Skipped),
            new(ConstVerificationStatus.Failure),
            new(ConstVerificationStatus.Failure),
            new(ConstVerificationStatus.Failure),
        } },
    };

    [Theory]
    [MemberData(nameof(EnumerateVerifications_Data))]
    public void EnumerateVerifications(UnitTestResult a, ConstVerification[] expected)
    {
        Assert.Equal(expected, a.EnumerateVerifications());
    }

    public static readonly TheoryData<string, Dictionary<string, UnitTestResult>> Parse_Data = new()
    {
        { """
            Class, success , skipped ,failure
            Foo,1,2,3
            Foo,10,10,10
            Bar,10,10,10
            """,
            new (){
                { "Foo", new("Foo", 11, 12, 13) },
                { "Bar", new("Bar", 10, 10, 10) },
            }
        }
    };

    [Theory]
    [MemberData(nameof(Parse_Data))]
    public void Parse(string input, Dictionary<string, UnitTestResult> expected)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
        Assert.Equal(expected, UnitTestResult.Parse(ms));
    }
}
