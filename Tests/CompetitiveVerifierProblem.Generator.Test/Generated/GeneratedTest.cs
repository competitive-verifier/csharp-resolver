using Newtonsoft.Json;
using System.Text;

namespace CompetitiveVerifierProblem.Generator.Test.Generated;
public class GeneratedTest
{
    [Fact]
    public void Enumerate()
    {
        var sb = new StringBuilder();
        using (var sw = new StringWriter(sb))
        {
            Console.SetOut(sw);
            Program.Main([]);
        }
        var writtern = sb.ToString();
        var obj = JsonConvert.DeserializeObject<Dictionary<string, ProblemJson[]>>(writtern)!;
        var aplusb = Assert.Single(obj["Aplusb"]);
        var pi = Assert.Single(obj["Pi"]);

        Assert.Equal("problem", aplusb.Type);
        Assert.Equal("problem", pi.Type);

        Assert.Equal("https://judge.yosupo.jp/problem/aplusb", aplusb.Problem);
        Assert.Equal("https://example.com/pi", pi.Problem);


        Assert.Null(aplusb.Error);
        Assert.Equal(1e-9, pi.Error);

        Assert.Null(aplusb.Tle);
        Assert.Equal(12.3, pi.Tle);
    }
    class ProblemJson
    {
        [JsonProperty("type")]
        public string? Type { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("problem")]
        public string? Problem { get; set; }
        [JsonProperty("command")]
        public string? Command { get; set; }
        [JsonProperty("error")]
        public double? Error { get; set; }
        [JsonProperty("tle")]
        public double? Tle { get; set; }
    }
}