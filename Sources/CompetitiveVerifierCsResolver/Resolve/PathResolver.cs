using CompetitiveVerifierCsResolver.Models;
using System.Runtime.InteropServices;

namespace CompetitiveVerifierCsResolver.Resolve;
public interface IPathResolver
{
    string? RelativePath(string path);
}
internal class PathResolver : IPathResolver
{
    public PathResolver(string baseDir, GlobCollection include, GlobCollection enclude)
    {
        BaseDir = baseDir;
        Include = include;
        Exclude = enclude;
    }
    private string BaseDir { get; }
    private GlobCollection Include { get; }
    private GlobCollection Exclude { get; }
    private readonly Dictionary<string, string?> targetCache = [];

    IEnumerable<string> GetParents(string path, bool includeSelf = true)
    {
        if (includeSelf)
            yield return path;

        for (var di = Directory.GetParent(path); di is not null; di = di.Parent)
            yield return Path.GetRelativePath(BaseDir, di.FullName);
    }

    string? RelativePathImpl(string path)
    {
        if (!Path.IsPathFullyQualified(path)) return null;
        path = Path.GetRelativePath(BaseDir, path);
        if (path.StartsWith('.') || Path.IsPathFullyQualified(path)) return null;

        string? result = null;

        foreach (var p in GetParents(path))
        {
            if (Exclude.IsMatch(p)) return null;
            if (Include.IsMatch(p)) result = path;
        }

        return result?.Replace('\\', '/');
    }
    public string? RelativePath(string path)
    {
        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault(targetCache, path, out var exists);
        if (!exists)
        {
            result = RelativePathImpl(path);
        }
        return result;
    }
}
