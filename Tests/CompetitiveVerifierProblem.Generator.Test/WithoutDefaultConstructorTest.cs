using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;

namespace CompetitiveVerifierProblem.Generator.Test;

public class WithoutDefaultConstructorTest : TestBase
{
    static readonly (string filename, string content)[] Sources = [
                        (
                            @"/home/mine/HelloWorldAoj.cs",
                            """
internal class HelloWorldAoj : CompetitiveVerifier.ProblemSolver
{
    public override string Url => "https://onlinejudge.u-aizu.ac.jp/courses/lesson/2/ITP1/1/ITP1_1_A";
    public override void Solve()
    {
        global::System.Console.WriteLine("Hello World");
    }
}
"""
                        ),
                        (
                            @"/home/mine/HelloWorldAoj2.cs",
                            """
namespace Space{
internal class HelloWorldAoj2 : CompetitiveVerifier.ProblemSolver
{
    public HelloWorldAoj2(int num){}
    public override string Url => "https://onlinejudge.u-aizu.ac.jp/courses/lesson/2/ITP1/1/ITP1_1_A";
    public override void Solve()
    {
        global::System.Console.WriteLine("Hello World");
    }
}
}
"""
                        ),
                    ];

    static readonly (Type sourceGeneratorType, string filename, string content)[] GeneratedSources =
    [
        .. ConstantGeneratedSources,
        (typeof(ProblemGenerator), "Main.impl.cs", """
                        #pragma warning disable IDE0161,CS8602
                        internal partial class Program
                        {
                            static partial void Enumerate()
                            {
                                var classes = new global::CompetitiveVerifier.ProblemSolver[]
                                {
                        new HelloWorldAoj(),

                                };

                                bool isFirst = true;
                                global::System.Console.Write('{');
                                foreach(var c in classes)
                                {
                                    if (isFirst)
                                        isFirst = false;
                                    else
                                        global::System.Console.Write(',');
                                    global::System.Console.Write('"');
                                    global::System.Console.Write(c.GetType().FullName);
                                    global::System.Console.Write('"');
                                    global::System.Console.Write(':');
                                    global::System.Console.Write('[');
                                    global::System.Console.Write(c.ToJson());
                                    global::System.Console.Write(']');
                                }
                                global::System.Console.WriteLine('}');
                            }

                            static partial void Run(string className)
                            {
                                GetSolver(className).Solve();
                            }

                            static global::CompetitiveVerifier.ProblemSolver GetSolver(string className)
                            {
                                switch(className)
                                {
                        case "HelloWorldAoj":return new HelloWorldAoj();
                        
                                    default: throw new global::System.ArgumentException($"{className} is not found.", nameof(className));
                                }
                            }
                        }
                        """.ReplaceLineEndings()
        ),
    ];

    [Test]
    public async Task Default(CancellationToken cancellationToken)
    {
        var test = new GeneratorTest
        {
            TestState =
                {
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("VERIFY0002")
                            .WithSpan("/home/mine/HelloWorldAoj2.cs", 2, 16, 2, 30)
                            .WithArguments("Space.HelloWorldAoj2"),
                    },
                    OutputKind = OutputKind.ConsoleApplication,
                }
        };
        foreach (var tup in Sources) test.TestState.Sources.Add(tup);
        foreach (var tup in GeneratedSources) test.TestState.GeneratedSources.Add(tup);

        await test.RunAsync(cancellationToken);
    }
}