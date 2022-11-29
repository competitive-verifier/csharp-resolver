using Microsoft.CodeAnalysis.MSBuild;

namespace CompetitiveCsResolver;
public class CompetitiveCsResolverCommand : ConsoleAppBase
{
    [RootCommand]
    public async Task Resolve(
    [Option(0, "Specify solution path")] string solutionPath)
    {
        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath, cancellationToken: Context.CancellationToken);
        Console.WriteLine(solution.Projects.Count());
    }
}
