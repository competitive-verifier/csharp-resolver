using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Sdk;

namespace CompetitiveVerifierCsResolver.Verifier;

public class VerificationTest
{
    public readonly static TheoryData<ParseResult, string> Json_Data = new()
    {
        {
            new ParseResult
            {
                {
                    "Foo",
                    [
                        new ProblemVerification("http://example.com/foo","dotnet Foo",Name:"C#(test)",1e-9,12.3),
                    ]
                },
                {
                    "Bar",
                    [
                        new ProblemVerification("http://example.com/bar","dotnet Bar"),
                    ]
                },
            },
            """{"Bar":[{"type":"problem","problem":"http://example.com/bar","command":"dotnet Bar"}],"Foo":[{"type":"problem","name":"C#(test)","problem":"http://example.com/foo","command":"dotnet Foo","error":1E-09,"tle":12.3}]}"""
        }
    };

    static readonly JsonSerializerOptions ignoreNullOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public class ParseResult : SortedDictionary<string, ProblemVerification[]>, IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info)
        {
            foreach (var (k, v) in JsonSerializer.Deserialize<Dictionary<string, ProblemVerification[]>>(info.GetValue<string>("json")!)!)
            {
                this[k] = v;
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("json", JsonSerializer.Serialize(this));
        }
    }

    [Theory]
    [MemberData(nameof(Json_Data))]
    public void ToJson(SortedDictionary<string, ProblemVerification[]> dict, string json)
    {
        Assert.Equal(json, JsonSerializer.Serialize(dict, ignoreNullOptions));
    }

    [Theory]
    [MemberData(nameof(Json_Data))]
    public void Parse(SortedDictionary<string, ProblemVerification[]> dict, string json)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        Assert.Equal(dict, ProblemVerification.Parse(ms)!);
    }
}