using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierProblem.Generator.Test.Generated;

public partial class GeneratedTest
{
    [Test]
    public async Task Enumerate()
    {
        Program.Main([]);
        var writtern = TestContext.Current!.GetStandardOutput();
        var obj = JsonSerializer.Deserialize(writtern, ProblemJsonDictContext.Default.DictionaryStringProblemJsonArray)!;

        await Assert.That(obj).Count().IsEqualTo(2);

        var aplusb = await Assert.That(obj["Aplusb"]).HasSingleItem();
        var pi = await Assert.That(obj["Example.Pi"]).HasSingleItem();
        var command = await Assert.That<string>(() => CompetitiveVerifier.ProblemSolver.ResolveRuntimeInfo().Command)
            .ThrowsNothing()
            .And.IsNotNull();

        await Assert.That(aplusb.Command).IsEqualTo(command + " Aplusb");
        await Assert.That(pi.Command).IsEqualTo(command + " Example.Pi");

        await Assert.That(aplusb)
            .Satisfies(p => p!.Command!.StartsWith("dotnet ")).Or.Satisfies(p => p!.Name!.Contains("AOT"));
        await Assert.That(aplusb)
            .Satisfies(p => !p!.Command!.StartsWith("dotnet ")).Or.Satisfies(p => !p!.Name!.Contains("AOT"));
        await Assert.That(pi)
            .Satisfies(p => p!.Command!.StartsWith("dotnet ")).Or.Satisfies(p => p!.Name!.Contains("AOT"));
        await Assert.That(pi)
            .Satisfies(p => !p!.Command!.StartsWith("dotnet ")).Or.Satisfies(p => !p!.Name!.Contains("AOT"));

        await Assert.That(aplusb.Type).IsEqualTo("problem");
        await Assert.That(pi.Type).IsEqualTo("problem");

        await Assert.That(aplusb.Problem).IsEqualTo("https://judge.yosupo.jp/problem/aplusb");
        await Assert.That(pi.Problem).IsEqualTo("https://example.com/pi");

        await Assert.That(aplusb.Error).IsNull();
        await Assert.That(pi.Error).IsEqualTo(1e-9);

        await Assert.That(aplusb.Tle).IsNull();
        await Assert.That(pi.Tle).IsEqualTo(12.3);
    }

    [JsonSerializable(typeof(Dictionary<string, ProblemJson[]>))]
    partial class ProblemJsonDictContext : JsonSerializerContext;
    class ProblemJson
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("problem")]
        public string? Problem { get; set; }
        [JsonPropertyName("command")]
        public string? Command { get; set; }
        [JsonPropertyName("error")]
        public double? Error { get; set; }
        [JsonPropertyName("tle")]
        public double? Tle { get; set; }
    }
}