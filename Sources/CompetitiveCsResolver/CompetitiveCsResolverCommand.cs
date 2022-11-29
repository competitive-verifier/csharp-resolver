using Microsoft.CodeAnalysis.MSBuild;

namespace CompetitiveCsResolver;
public class CompetitiveCsResolverCommand : ConsoleAppBase
{
    [RootCommand]
    public async Task Resolve(
    [Option(0, "Specify solution path")] string solutionPath,
    [Option("unittest", "Specify unittest result csv path")] string? unittestResultFilePath,
    [Option("problems", "Specify output of CompetitiveVerifierProblem")] string? problemFilePath
        )
    {
        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath, cancellationToken: Context.CancellationToken);
        Console.WriteLine(solution.Projects.Count());
    }
}
