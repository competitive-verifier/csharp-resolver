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
                        #pragma warning disable IDE0161,CS8602
                        namespace CompetitiveVerifier
                        {
                            using Newtonsoft.Json;
                        
                            internal abstract class ProblemSolver
                            {
                                public abstract string Url { get; }
                                public virtual double? Error => null;
                                public virtual double? Tle => null;
                        
                                public abstract void Solve();
                                public string ToJson()
                                {
                                    return JsonConvert.SerializeObject(new JsonDataContract
                                    {
                                        Type = "problem",
                                        Url = Url,
                                        Command = $"dotnet {System.Reflection.Assembly.GetEntryAssembly().Location} {GetType().FullName}",
                                        Error = Error,
                                        Tle = Tle,
                                    }, Formatting.None);
                                }
                                [JsonObject]
                                private struct JsonDataContract
                                {
                                    [JsonProperty("type", Required = Required.DisallowNull)]
                                    public string Type { set; get; }
                                    [JsonProperty("problem", Required = Required.DisallowNull)]
                                    public string Url { set; get; }
                                    [JsonProperty("command", Required = Required.DisallowNull)]
                                    public string Command { set; get; }
                                    [JsonProperty("error", Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
                                    public double? Error { set; get; }
                                    [JsonProperty("tle", Required = Required.AllowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
                                    public double? Tle { set; get; }
                                }
                            }
                        }
                        """),

                        ("Main.cs", """
                        #pragma warning disable IDE0161,CS8602
                        internal partial class Program
                        {
                            static void Main(string[] args)
                            {
                                if (args.Length > 0)
                                {
                                    var a = args[0];
                                    if (a == "-h" || a == "--help")
                                    {
                                        System.Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                                        System.Console.WriteLine();
                                        System.Console.WriteLine(@"Options:
                        -i, --interactive   Run interactive mode.
                        -h, --help          Show this help.");
                                        return;
                                    }
                                    if (a == "-i" || a == "--interactive")
                                    {
                                        System.Console.WriteLine(@"Input class name");
                                        string line;
                                        do
                                        {
                                            line = System.Console.ReadLine().Trim();
                                        }
                                        while(line == "");
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