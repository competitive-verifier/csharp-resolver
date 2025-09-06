using DotNet.Globbing;

namespace CompetitiveVerifierCsResolver.Models;
public class GlobCollectionTest
{
    public static TheoryData<GlobCollection, string, bool> IsMatch_Data()
    {
        var td = new TheoryData<GlobCollection, string, bool>();
        {
            var globs = Get("**/*foo.cs");
            td.Add(globs, "/foo.cs", true);
            td.Add(globs, "/home/dir/foo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cpp", false);
        }
        {
            var globs = Get("**/*foo.cs", "myfoo.cpp");
            td.Add(globs, "/foo.cs", true);
            td.Add(globs, "/home/dir/foo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cs", true);
            td.Add(globs, "/home/dir/myfoo.cpp", false);
        }
        return td;
    }

    [Theory]
    [MemberData(nameof(IsMatch_Data))]
    public void IsMatch(GlobCollection globs, string input, bool isMatch)
    {
        Assert.Equal(isMatch, globs.IsMatch(input));
    }
    static GlobCollection Get(params string[] patterns) => new(patterns.Select(Glob.Parse));
}
