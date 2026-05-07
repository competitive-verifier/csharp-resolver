using System;
using System.ComponentModel;
using System.Reflection;
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
                Command = $"{runtimeINfo.Command} {GetType().FullName}",
                Error = Error,
                Tle = Tle,
            }, SerializerContext.JsonDataContract);
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RuntimeInfo(Assembly entryAssembly, string nativeCommand)
    {
        public Assembly? EntryAssembly { get; } = entryAssembly;
        public string? NativeCommand { get; } = nativeCommand;

        public bool IsNative => NativeCommand == Command;
        public string Command => (EntryAssembly, NativeCommand) switch
        {
            ({ Location: { Length: > 0 } loc }, _) => $"dotnet {loc}",
            (_, { } native) => native,
            _ => throw new InvalidOperationException("Cannot determine the location of the executing assembly."),
        };
    }
}