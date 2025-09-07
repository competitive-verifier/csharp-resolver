using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;
public record VerificationInput(
        [property: JsonPropertyName("files"), JsonRequired] ImmutableSortedDictionary<string, VerificationFile> Files)
{
    public string ToJson() => JsonSerializer.Serialize(this, VerificationJsonContext.IgnoreNull.VerificationInput);
}
