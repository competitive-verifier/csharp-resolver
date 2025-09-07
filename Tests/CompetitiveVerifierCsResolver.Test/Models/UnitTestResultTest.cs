using CompetitiveVerifierCsResolver.Verifier;
using System.Text;
using Xunit.Sdk;

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

    public static readonly TheoryData<UnitTestResult, ConstVerificationStatus[]> EnumerateVerifications_Data = new()
    {
        { new("Foo", 1, 2, 3), [
            ConstVerificationStatus.Success,
            ConstVerificationStatus.Skipped,
            ConstVerificationStatus.Skipped,
            ConstVerificationStatus.Failure,
            ConstVerificationStatus.Failure,
            ConstVerificationStatus.Failure,
        ] },
    };

    [Theory]
    [MemberData(nameof(EnumerateVerifications_Data))]
    public void EnumerateVerifications(UnitTestResult a, ConstVerificationStatus[] expected)
    {
        Assert.Equal(expected.Select(s => new ConstVerification(s)), a.EnumerateVerifications());
    }

    public class ParseResult : Dictionary<string, UnitTestResult>, IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info)
        {
            var count = info.GetValue<int>(nameof(Count));
            for (int i = 0; i < count; i++)
            {
                var k = info.GetValue<string>($"Key{i}")!;
                (string Name, int Success, int Skipped, int Failure)
                    = info.GetValue<(string Name, int Success, int Skipped, int Failure)>($"Value{i}");

                this[k] = new(Name, Success, Skipped, Failure);
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Count), Count);
            int i = 0;
            foreach ((var k, (string Name, int Success, int Skipped, int Failure)) in this)
            {
                info.AddValue($"Key{i}", k);
                info.AddValue($"Value{i++}", (Name, Success, Skipped, Failure));
            }
        }
    }
    public static readonly TheoryData<string, ParseResult> Parse_Data = new()
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
