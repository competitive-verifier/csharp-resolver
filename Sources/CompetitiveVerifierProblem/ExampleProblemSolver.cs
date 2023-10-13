#pragma warning disable IDE0001, IDE0161, CS8602
// Example of ProblemSolver

namespace CompetitiveVerifier
{
    internal abstract class ProblemSolver
    {
        public abstract string Url { get; }
        public virtual double? Error => null;
        public virtual double? Tle => null;

        public abstract void Solve();
        public string ToJson()
        {
            var thisLocation = global::System.Reflection.Assembly.GetEntryAssembly().Location;
            return global::Newtonsoft.Json.JsonConvert.SerializeObject(new JsonDataContract
            {
                Type = "problem",
                Name = $"C#({System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription})",
                Url = Url,
                Command = $"dotnet {thisLocation} {GetType().Name}",
                Error = Error,
                Tle = Tle,
            }, global::Newtonsoft.Json.Formatting.None);
        }
        [global::Newtonsoft.Json.JsonObject]
        private struct JsonDataContract
        {
            [global::Newtonsoft.Json.JsonProperty("type", Required = global::Newtonsoft.Json.Required.DisallowNull)]
            public string Type { set; get; }
            [global::Newtonsoft.Json.JsonProperty("name", Required = global::Newtonsoft.Json.Required.AllowNull)]
            public string Name { set; get; }
            [global::Newtonsoft.Json.JsonProperty("problem", Required = global::Newtonsoft.Json.Required.DisallowNull)]
            public string Url { set; get; }
            [global::Newtonsoft.Json.JsonProperty("command", Required = global::Newtonsoft.Json.Required.DisallowNull)]
            public string Command { set; get; }
            [global::Newtonsoft.Json.JsonProperty("error", Required = global::Newtonsoft.Json.Required.AllowNull, DefaultValueHandling = global::Newtonsoft.Json.DefaultValueHandling.Ignore)]
            public double? Error { set; get; }
            [global::Newtonsoft.Json.JsonProperty("tle", Required = global::Newtonsoft.Json.Required.AllowNull, DefaultValueHandling = global::Newtonsoft.Json.DefaultValueHandling.Ignore)]
            public double? Tle { set; get; }
        }
    }
}