using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;

public class VerificationTest
{
    public readonly static IEnumerable<(ParseResult, string)> Json_Data =
    [
        (new ParseResult
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
            }, """{"Bar":[{"type":"problem","problem":"http://example.com/bar","command":"dotnet Bar"}],"Foo":[{"type":"problem","name":"C#(test)","problem":"http://example.com/foo","command":"dotnet Foo","error":1E-09,"tle":12.3}]}""")
    ];

    static readonly JsonSerializerOptions ignoreNullOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public class ParseResult : SortedDictionary<string, ProblemVerification[]>;

    [Test]
    [MethodDataSource(nameof(Json_Data))]
    public async Task ToJson(SortedDictionary<string, ProblemVerification[]> dict, string json)
    {
        await Assert.That(JsonSerializer.Serialize(dict, ignoreNullOptions)).IsEqualTo(json);
    }

    [Test]
    [MethodDataSource(nameof(Json_Data))]
    public async Task Parse(SortedDictionary<string, ProblemVerification[]> dict, string json)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await Assert.That(ProblemVerification.Parse(ms)!).IsEquivalentTo(dict);
    }
}