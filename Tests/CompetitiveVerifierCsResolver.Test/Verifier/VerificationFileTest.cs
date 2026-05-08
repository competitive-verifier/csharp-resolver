using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CompetitiveVerifierCsResolver.Verifier;

public class VerificationFileTest
{
    public static readonly IEnumerable<(VerificationFile, VerificationFile, VerificationFile)> Merge_Data =
    [
        (new(
                ["Bar", "Baz"],
                ImmutableSortedDictionary.CreateRange(
                [
                    KeyValuePair.Create<string, object>("title", "foo"),
                    KeyValuePair.Create<string, object>("links", new[]{"http://example.com/foo"}),
                ]),
                [new ProblemVerification("http://example.com/foo", "dotnet foo")]
            ), new(
                ["Bar", "FooBar"],
                ImmutableSortedDictionary.CreateRange(
                [
                    KeyValuePair.Create<string, object>("title", "foo"),
                    KeyValuePair.Create<string, object>("links", new[]{"http://example.com/foo"}),
                ]),
                [new ConstVerification(ConstVerificationStatus.Success)]
            ), new(
                ["Bar", "Baz", "FooBar"],
                ImmutableSortedDictionary.CreateRange(
                [
                    KeyValuePair.Create<string, object>("title", "foo"),
                    KeyValuePair.Create<string, object>("links", new[]{"http://example.com/foo"}),
                ]),
                [new ProblemVerification("http://example.com/foo", "dotnet foo"), new ConstVerification(ConstVerificationStatus.Success)]
            )),
    ];

    [Test]
    [MethodDataSource(nameof(Merge_Data))]
    public async Task Merge(VerificationFile a, VerificationFile b, VerificationFile expected)
    {
        var merged = a.Merge(b);
        await Assert.That(merged.Dependencies).IsEquivalentTo(expected.Dependencies);
        await Assert.That(merged.DocumentAttributes).IsEquivalentTo(expected.DocumentAttributes);
        await Assert.That(merged.Verification.AsEnumerable()).IsEquivalentTo(expected.Verification.AsEnumerable());
    }
}