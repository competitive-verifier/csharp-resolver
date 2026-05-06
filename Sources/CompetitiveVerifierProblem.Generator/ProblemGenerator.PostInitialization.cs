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
        private static bool _isNative = false;
        private static string _cache_command = null;
    
#if NET6_0_OR_GREATER
        [global::System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "Working as expected")]
#endif
        protected override global::CompetitiveVerifier.Core.ProblemSolverBase.RuntimeInfo GetRuntimeInfo()
        {
            if (_cache_command == null)
            {
                var dllLocation = global::System.Reflection.Assembly.GetEntryAssembly().Location;
                _cache_command = $"dotnet {dllLocation}";
                if (string.IsNullOrEmpty(dllLocation))
                {
#if NET6_0_OR_GREATER
                    _cache_command = global::System.Environment.ProcessPath;
                    if(!global::System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported
                     && _cache_command != null)
                    {
                        _isNative = true;
                    }
                    else
#endif
                    {
                        throw new global::System.InvalidOperationException("Cannot determine the location of the executing assembly.");
                    }
                }
            }
            return new global::CompetitiveVerifier.Core.ProblemSolverBase.RuntimeInfo(_cache_command, _isNative);
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
                global::System.Console.WriteLine(global::System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
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