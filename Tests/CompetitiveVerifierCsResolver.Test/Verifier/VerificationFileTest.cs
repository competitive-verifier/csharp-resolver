using System.Collections.Immutable;

namespace CompetitiveVerifierCsResolver.Verifier;
public class VerificationFileTest
{
    public static readonly TheoryData<VerificationFile, VerificationFile, VerificationFile> Merge_Data = new()
    {
        {
            new(
                ImmutableHashSet.Create("Bar","Baz"),
                ImmutableSortedDictionary.CreateRange(new[]
                {
                    KeyValuePair.Create<string, object>("title", "foo"),
                    KeyValuePair.Create<string, object>("links", new[]{"http://example.com/foo"}),
                }),
                ImmutableArray.Create<Verification>(
                    new ProblemVerification("http://example.com/foo", "dotnet foo")
                )
            ),
            new(
                ImmutableHashSet.Create("Bar","FooBar"),
                ImmutableSortedDictionary.CreateRange(new[]
                {
                    KeyValuePair.Create<string, object>("title", "foo"),
                    KeyValuePair.Create<string, object>("links", new[]{"http://example.com/foo"}),
                }),
                ImmutableArray.Create<Verification>(
                    new ConstVerification(ConstVerificationStatus.Success)
                )
            ),
            new(
                ImmutableHashSet.Create("Bar","Baz","FooBar"),
                ImmutableSortedDictionary.CreateRange(new[]
                {
                    KeyValuePair.Create<string, object>("title", "foo"),
                    KeyValuePair.Create<string, object>("links", new[]{"http://example.com/foo"}),
                }),
                ImmutableArray.Create<Verification>(
                    new ProblemVerification("http://example.com/foo", "dotnet foo"),
                    new ConstVerification(ConstVerificationStatus.Success)
                )
            )
        },
    };

    [Theory]
    [MemberData(nameof(Merge_Data))]
    public void Merge(VerificationFile a, VerificationFile b, VerificationFile expected)
    {
        var merged = a.Merge(b);
        Assert.Equal(expected.Dependencies, merged.Dependencies);
        Assert.Equal(expected.DocumentAttributes, merged.DocumentAttributes);
        Assert.Equal(expected.Verification.AsEnumerable(), merged.Verification.AsEnumerable());
    }

}