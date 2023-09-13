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


    public static (string filename, string content)[] ConstantSources => new[]
        {
                        ("ProblemSolver.cs", """
                        #pragma warning disable IDE0001, IDE0161, CS8602
                        namespace CompetitiveVerifier
                        {
                            internal abstract class ProblemSolver
                            {
                                public abstract string Url { get; }
                                public virtual double? Error => null;
                                public virtual double? Tle => null;

                                public abstract void Solve();
                                public string ToJson()
                                {
                                    var thisLocation = global::System.Reflection.Assembly.GetEntryAssembly().Location;
                                    return global::Newtonsoft.Json.JsonConvert.SerializeObject(new JsonDataContract
                                    {
                                        Type = "problem",
                                        Url = Url,
                                        Command = $"dotnet {thisLocation} {GetType().FullName}",
                                        Error = Error,
                                        Tle = Tle,
                                    }, global::Newtonsoft.Json.Formatting.None);
                                }
                                [global::Newtonsoft.Json.JsonObject]
                                private struct JsonDataContract
                                {
                                    [global::Newtonsoft.Json.JsonProperty("type", Required = global::Newtonsoft.Json.Required.DisallowNull)]
                                    public string Type { set; get; }
                                    [global::Newtonsoft.Json.JsonProperty("problem", Required = global::Newtonsoft.Json.Required.DisallowNull)]
                                    public string Url { set; get; }
                                    [global::Newtonsoft.Json.JsonProperty("command", Required = global::Newtonsoft.Json.Required.DisallowNull)]
                                    public string Command { set; get; }
                                    [global::Newtonsoft.Json.JsonProperty("error", Required = global::Newtonsoft.Json.Required.AllowNull, DefaultValueHandling = global::Newtonsoft.Json.DefaultValueHandling.Ignore)]
                                    public double? Error { set; get; }
                                    [global::Newtonsoft.Json.JsonProperty("tle", Required = global::Newtonsoft.Json.Required.AllowNull, DefaultValueHandling = global::Newtonsoft.Json.DefaultValueHandling.Ignore)]
                                    public double? Tle { set; get; }
                                }
                            }
                        }
                        """),

                        ("Main.cs", """
                        #pragma warning disable IDE0001, IDE0161, CS8602
                        internal partial class Program
                        {
                            static void Main(string[] args)
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
    };
}