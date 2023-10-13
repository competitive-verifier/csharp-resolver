using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;

public class VerificationTest
{
    public readonly static TheoryData Json_Data = new TheoryData<SortedDictionary<string, ProblemVerification[]>, string>
    {
        {
            new SortedDictionary<string, ProblemVerification[]>
            {
                {
                    "Foo",
                    new ProblemVerification[]
                    {
                        new ProblemVerification("http://example.com/foo","dotnet Foo",Name:"C#(test)"),
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
            """{"Bar":[{"type":"problem","problem":"http://example.com/bar","command":"dotnet Bar"}],"Foo":[{"type":"problem","name":"C#(test)","problem":"http://example.com/foo","command":"dotnet Foo"}]}"""
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