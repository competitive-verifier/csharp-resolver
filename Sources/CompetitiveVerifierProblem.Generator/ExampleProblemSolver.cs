#pragma warning disable IDE0161
// Example of ProblemSolver

#nullable disable
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
            return JsonConvert.SerializeObject(new JsonDataContract(this), Formatting.None);
        }
        [JsonObject]
        private struct JsonDataContract
        {
            [JsonProperty("type", Required = Required.DisallowNull)]
            public string Type { get; } = "problem";
            [JsonProperty("problem", Required = Required.DisallowNull)]
            public string Url { get; }
            [JsonProperty("command", Required = Required.DisallowNull)]
            public string Command { get; }
            [JsonProperty("error", Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public double? Error { get; }

            public JsonDataContract(ProblemSolver solver)
            {
                Type = "problem";
                Url = solver.Url;
                Command = $"dotnet {System.Reflection.Assembly.GetEntryAssembly().Location} {solver.GetType().FullName}";
                Error = solver.Error;
            }
        }
    }
}