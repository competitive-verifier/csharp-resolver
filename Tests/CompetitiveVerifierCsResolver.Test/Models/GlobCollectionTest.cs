using DotNet.Globbing;
using System.Collections.Immutable;

namespace CompetitiveVerifierCsResolver.Models;

public class GlobCollectionTest
{
    public static IEnumerable<(ImmutableArray<string>, string, bool)> IsMatch_Data()
    {
        {
            ImmutableArray<string> globs = ["**/*foo.cs"];
            yield return (globs, "/foo.cs", true);
            yield return (globs, "/home/dir/foo.cs", true);
            yield return (globs, "/home/dir/myfoo.cs", true);
            yield return (globs, "/home/dir/myfoo.cpp", false);
        }
        {
            ImmutableArray<string> globs = ["**/*foo.cs", "myfoo.cpp"];
            yield return (globs, "/foo.cs", true);
            yield return (globs, "/home/dir/foo.cs", true);
            yield return (globs, "/home/dir/myfoo.cs", true);
            yield return (globs, "/home/dir/myfoo.cpp", false);
        }
    }

    [Test]
    [MethodDataSource(nameof(IsMatch_Data))]
    public async Task IsMatch(ImmutableArray<string> globs, string input, bool isMatch)
    {
        await Assert.That(new GlobCollection(globs.Select(Glob.Parse)).IsMatch(input)).IsEqualTo(isMatch);
    }
}