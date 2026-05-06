using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifier.Core
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(ProblemSolverBase.JsonDataContract))]
    internal partial class SolverContext : JsonSerializerContext;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class ProblemSolverBase
    {
        public abstract string Url { get; }
        public virtual double? Error => null;
        public virtual double? Tle => null;

        public abstract void Solve();
        protected abstract RuntimeInfo GetRuntimeInfo();

        private static readonly SolverContext SerializerContext = new(new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false,
        });
        public string ToJson()
        {
            var runtimeINfo = GetRuntimeInfo();
            var aot = runtimeINfo.IsNative ? " AOT" : "";
            return JsonSerializer.Serialize(new JsonDataContract
            {
                Type = "problem",
                Name = $"C#({System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}{aot})",
                Url = Url,
                Command = $"{runtimeINfo.Command} {GetType().Name}",
                Error = Error,
                Tle = Tle,
            }, SerializerContext.JsonDataContract);
        }
        protected class RuntimeInfo(string command, bool isNative)
        {
            public string Command { get; } = command;
            public bool IsNative { get; } = isNative;
        }
        internal struct JsonDataContract
        {
            [JsonPropertyName("type")]
            [JsonRequired]
            public string Type { set; get; }
            [JsonPropertyName("name")]
            public string? Name { set; get; }
            [JsonPropertyName("problem")]
            [JsonRequired]
            public string Url { set; get; }
            [JsonPropertyName("command")]
            [JsonRequired]
            public string Command { set; get; }
            [JsonPropertyName("error")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public double? Error { set; get; }
            [JsonPropertyName("tle")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public double? Tle { set; get; }
        }
    }
}