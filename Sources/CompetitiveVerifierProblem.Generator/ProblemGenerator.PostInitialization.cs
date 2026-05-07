using Microsoft.CodeAnalysis;

namespace CompetitiveVerifierProblem;


public partial class ProblemGenerator : IIncrementalGenerator
{
    private void PostInitialization(IncrementalGeneratorPostInitializationContext context)
    {
        var token = context.CancellationToken;
        token.ThrowIfCancellationRequested();

        foreach (var (name, source) in ConstantSources)
        {
            context.AddSource(name, source);
            token.ThrowIfCancellationRequested();
        }
    }


    public static (string filename, string content)[] ConstantSources =>
        [
                        ("ProblemSolver.cs",
"""
#pragma warning disable
namespace CompetitiveVerifier
{
    internal abstract class ProblemSolver : global::CompetitiveVerifier.Core.ProblemSolverBase
    {
        private static global::CompetitiveVerifier.Core.RuntimeInfo _cache_runtimeinfo = null;
    
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "Working as expected")]
#endif
        protected override global::CompetitiveVerifier.Core.RuntimeInfo GetRuntimeInfo()
        {
            return ResolveRuntimeInfo();
        }
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "Working as expected")]
#endif
        internal static global::CompetitiveVerifier.Core.RuntimeInfo ResolveRuntimeInfo()
        {
            if (_cache_runtimeinfo == null)
            {
                _cache_runtimeinfo = new global::CompetitiveVerifier.Core.RuntimeInfo(global::System.Reflection.Assembly.GetEntryAssembly(), 
#if NET6_0_OR_GREATER
                    global::System.Environment.ProcessPath
#else
                    null
#endif
                );
            }
            return _cache_runtimeinfo;
        }
    }
}
"""),

                        ("Main.cs",
"""
#pragma warning disable IDE0001, IDE0161, CS8602
internal partial class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            var a = args[0];
            if (a == "-h" || a == "--help")
            {
                var commandName = global::System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Name;
#if NET6_0_OR_GREATER
                var processPath = global::System.Environment.ProcessPath;
                if (commandName == null && processPath != null)
                    commandName = global::System.IO.Path.GetFileNameWithoutExtension(processPath);
#endif
                global::System.Console.WriteLine(commandName);
                global::System.Console.WriteLine();
                global::System.Console.WriteLine(@"Options:
                        -i, --interactive   Run interactive mode.
                        -h, --help          Show this help.");
                return;
            }
            if (a == "-i" || a == "--interactive")
            {
                global::System.Console.WriteLine(@"Input class name");
                string line;
                do
                {
                    line = global::System.Console.ReadLine().Trim();
                }
                while (line == "");
                a = line;
            }
            Run(a);
        }
        else
        {
            Enumerate();
        }
    }
    static partial void Run(string className);
    static partial void Enumerate();
}
"""),
    ];
}