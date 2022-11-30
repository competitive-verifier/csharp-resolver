using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace CompetitiveCsResolver.Verifier;
public record VerificationFile(
        [property: JsonPropertyName("dependencies"), JsonRequired] ImmutableHashSet<string> Dependencies,
        [property: JsonPropertyName("document_attributes"), JsonRequired] ImmutableDictionary<string, string> DocumentAttributes,
        [property: JsonPropertyName("verification"), JsonRequired] ImmutableArray<Verification> Verification
        )
{
    public VerificationFile Merge(VerificationFile other)
    {
        var dependencies = Dependencies.Union(other.Dependencies);
        var documentAttributes = DocumentAttributes.Concat(other.DocumentAttributes)
            .GroupBy(p => p.Key, p => p.Value)
            .ToImmutableDictionary(g => g.Key, g => g.First());
        var verification = Verification.Concat(other.Verification).ToImmutableArray();
        return new VerificationFile(dependencies, documentAttributes, verification);
    }
}
