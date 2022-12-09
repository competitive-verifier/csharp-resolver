using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;
public record VerificationInput(
        [property: JsonPropertyName("files"), JsonRequired] ImmutableSortedDictionary<string, VerificationFile> Files)
{
    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    {
#if NET5_0_OR_GREATER
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
    });
}
