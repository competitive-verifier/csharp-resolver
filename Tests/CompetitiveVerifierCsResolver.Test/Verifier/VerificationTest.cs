using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;

public class VerificationTest
{
    public readonly static TheoryData<SortedDictionary<string, ProblemVerification[]>, string> Json_Data = new()
    {
        {
            new SortedDictionary<string, ProblemVerification[]>
            {
                {
                    "Foo",
                    new ProblemVerification[]
                    {
                        new ProblemVerification("http://example.com/foo","dotnet Foo",Name:"C#(test)",1e-9,12.3),
                    }
                },
                {
                    "Bar",
                    new ProblemVerification[]
                    {
                        new ProblemVerification("http://example.com/bar","dotnet Bar"),
                    }
                },
            },
            """{"Bar":[{"type":"problem","problem":"http://example.com/bar","command":"dotnet Bar"}],"Foo":[{"type":"problem","name":"C#(test)","problem":"http://example.com/foo","command":"dotnet Foo","error":1E-09,"tle":12.3}]}"""
        }
    };

    [Theory]
    [MemberData(nameof(Json_Data))]
    public void ToJson(SortedDictionary<string, ProblemVerification[]> dict, string json)
    {
        Assert.Equal(json, JsonSerializer.Serialize(dict, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        }));
    }

    [Theory]
    [MemberData(nameof(Json_Data))]
    public void Parse(SortedDictionary<string, ProblemVerification[]> dict, string json)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        Assert.Equal(dict, ProblemVerification.Parse(ms)!);
    }
}