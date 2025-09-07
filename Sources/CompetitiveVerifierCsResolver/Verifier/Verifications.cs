using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;

[JsonConverter(typeof(VerificationConverter))]
public abstract record Verification
{
    [JsonPropertyName("type"), JsonPropertyOrder(0)]
    public abstract string Type { get; }
}

public record ProblemVerification(
    [property: JsonPropertyOrder(2), JsonPropertyName("problem"), JsonRequired] string Url,
    [property: JsonPropertyOrder(3), JsonPropertyName("command"), JsonRequired] string Command,
    [property: JsonPropertyOrder(1), JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyOrder(4), JsonPropertyName("error")] double? Error = null,
    [property: JsonPropertyOrder(4), JsonPropertyName("tle")] double? Tle = null
) : Verification
{
    internal const string TypeVal = "problem";

    [JsonPropertyName("type"), JsonPropertyOrder(0)]
    public override string Type => TypeVal;

    public static Dictionary<string, ProblemVerification[]>? Parse(Stream stream)
    {
        return JsonSerializer.Deserialize(stream, VerificationJsonContext.IgnoreNull.DictionaryStringProblemVerificationArray);
    }
}

public record ConstVerification(
    [property: JsonPropertyOrder(1), JsonPropertyName("status"), JsonRequired] ConstVerificationStatus Status) : Verification
{
    internal const string TypeVal = "const";

    [JsonPropertyName("type"), JsonPropertyOrder(0)]
    public override string Type => TypeVal;
}

public class VerificationConverter : JsonConverter<Verification>
{
    public override Verification Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            document.RootElement.WriteTo(writer);
        }

        return document.RootElement.GetProperty("type").GetString() switch
        {
            ConstVerification.TypeVal => JsonSerializer.Deserialize<ConstVerification>(bufferWriter.WrittenSpan, options) as Verification,
            ProblemVerification.TypeVal => JsonSerializer.Deserialize<ProblemVerification>(bufferWriter.WrittenSpan, options),
            _ => throw new InvalidDataException(),
        } ?? throw new InvalidDataException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        Verification value,
        JsonSerializerOptions options) =>
           JsonSerializer.Serialize(writer, value, value.GetType(), options);
}


[JsonConverter(typeof(ConstVerificationStatusConverter))]
public enum ConstVerificationStatus
{
    Success,
    Skipped,
    Failure,
}
public class ConstVerificationStatusConverter : JsonStringEnumConverter<ConstVerificationStatus>
{
    public ConstVerificationStatusConverter() : base(JsonNamingPolicy.CamelCase) { }
}