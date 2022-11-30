using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace CompetitiveCsResolver.Verifier;
public record VerificationInput(
        [property: JsonPropertyName("files"), JsonRequired] ImmutableDictionary<string, VerificationFile> Files);
