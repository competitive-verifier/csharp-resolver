using DotNet.Globbing;

namespace CompetitiveVerifierCsResolver.Models;
public class GlobCollection(IEnumerable<Glob> globs) : List<Glob>(globs)
{
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
