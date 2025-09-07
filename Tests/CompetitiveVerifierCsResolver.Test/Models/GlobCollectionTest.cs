using DotNet.Globbing;

namespace CompetitiveVerifierCsResolver.Models;
public class GlobCollectionTest
{
    public static TheoryData<string[], string, bool> IsMatch_Data()
    {
        var td = new TheoryData<string[], string, bool>();
        {
            string[] globs = ["**/*foo.cs"];
            td.Add(globs, "/foo.cs", true);
            td.Add(globs, "/home/dir/foo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cpp", false);
        }
        {
            string[] globs = ["**/*foo.cs", "myfoo.cpp"];
            td.Add(globs, "/foo.cs", true);
            td.Add(globs, "/home/dir/foo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cpp", false);
        }
        return td;
    }

    [Theory]
    [MemberData(nameof(IsMatch_Data))]
    public void IsMatch(string[] globs, string input, bool isMatch)
    {
        Assert.Equal(isMatch, new GlobCollection(globs.Select(Glob.Parse)).IsMatch(input));
    }
}
