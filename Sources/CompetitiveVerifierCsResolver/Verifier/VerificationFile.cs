using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;
public record VerificationFile(
        [property: JsonPropertyOrder(0), JsonPropertyName("dependencies"), JsonRequired]
        ImmutableHashSet<string> Dependencies,
        [property: JsonPropertyOrder(1), JsonPropertyName("document_attributes"), JsonRequired]
        ImmutableSortedDictionary<string, object> DocumentAttributes,
        [property: JsonPropertyOrder(2), JsonPropertyName("verification"), JsonRequired]
        ImmutableArray<Verification> Verification
        )
{
    public VerificationFile Merge(VerificationFile other)
    {
        var dependencies = Dependencies.Union(other.Dependencies);
        var documentAttributes = DocumentAttributes.Concat(other.DocumentAttributes)
            .GroupBy(p => p.Key, p => p.Value)
            .ToImmutableSortedDictionary(g => g.Key, g => g.First());
        var verification = Verification.Concat(other.Verification).ToImmutableArray();
        return new VerificationFile(dependencies, documentAttributes, verification);
    }
}
