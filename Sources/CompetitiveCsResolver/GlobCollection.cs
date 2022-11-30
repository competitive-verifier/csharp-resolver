using DotNet.Globbing;

namespace CompetitiveCsResolver;
internal class GlobCollection : List<Glob>
{
    public GlobCollection(IEnumerable<Glob> globs) : base(globs) { }

    public bool IsMatch(string path)
    {
        foreach (var glob in this)
        {
            if (glob.IsMatch(path))
                return true;
        }
        return false;
    }
}
