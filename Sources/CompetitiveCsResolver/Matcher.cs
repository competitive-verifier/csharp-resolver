using System.Runtime.InteropServices;

namespace CompetitiveCsResolver;
internal record Matcher
(
    GlobCollection Include,
    GlobCollection Exclude
    )
{
    private readonly Dictionary<string, string?> targetCache = new();

    static IEnumerable<string> GetParents(string path, bool includeSelf = true)
    {
        if (includeSelf)
            yield return path;

        for (var di = Directory.GetParent(path); di is not null; di = di.Parent)
            yield return Path.GetRelativePath(Environment.CurrentDirectory, di.FullName);
    }

    string? RelativePathImpl(string path)
    {
        if (!Path.IsPathFullyQualified(path)) return null;
        path = Path.GetRelativePath(Environment.CurrentDirectory, path);
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
    public bool IsTargetPath(string path) => RelativePath(path) is not null;
}
