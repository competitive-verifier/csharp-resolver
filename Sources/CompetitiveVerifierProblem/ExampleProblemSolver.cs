#pragma warning disable IDE0161
// Example of ProblemSolver

namespace CompetitiveVerifier
{
    using Newtonsoft.Json;

    internal abstract class ProblemSolver
    {
        public abstract string Url { get; }
        public virtual double? Error => null;

        public abstract void Solve();
        public string ToJson()
        {
            return JsonConvert.SerializeObject(new JsonDataContract
            {
                Type = "problem",
                Url = Url,
                Command = $"dotnet {System.Reflection.Assembly.GetEntryAssembly().Location} {GetType().FullName}",
                Error = Error,
            }, Formatting.None);
        }
        [JsonObject]
        private struct JsonDataContract
        {
            [JsonProperty("type", Required = Required.DisallowNull)]
            public string Type { set; get; }
            [JsonProperty("problem", Required = Required.DisallowNull)]
            public string Url { set; get; }
            [JsonProperty("command", Required = Required.DisallowNull)]
            public string Command { set; get; }
            [JsonProperty("error", Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public double? Error { set; get; }
        }
    }
}