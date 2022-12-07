using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace CompetitiveVerifierProblem.Generator.Test;

public class WithoutDefaultConstructorTest : TestBase
{
    static readonly (string filename, string content)[] Sources = new[]{
                        (
                            @"/home/mine/HelloWorldAoj.cs",
                            """
internal class HelloWorldAoj : CompetitiveVerifier.ProblemSolver
{
    public override string Url => "https://onlinejudge.u-aizu.ac.jp/courses/lesson/2/ITP1/1/ITP1_1_A";
    public override void Solve()
    {
        System.Console.WriteLine("Hello World");
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
        System.Console.WriteLine("Hello World");
    }
}
}
"""
                        ),
                    };

    static readonly (Type sourceGeneratorType, string filename, string content)[] GeneratedSources = ConstantGeneratedSources.Append(
                        (typeof(ProblemGenerator), "Main.impl.cs", """
                        #pragma warning disable IDE0161,CS8602
                        internal partial class Program
                        {
                            static partial void Enumerate()
                            {
                                var classes = new CompetitiveVerifier.ProblemSolver[]
                                {
                        new HelloWorldAoj(),

                                };

                                bool isFirst = true;
                                System.Console.Write('{');
                                foreach(var c in classes)
                                {
                                    if (isFirst)
                                        isFirst = false;
                                    else
                                        System.Console.Write(',');
                                    System.Console.Write('"');
                                    System.Console.Write(c.GetType().FullName);
                                    System.Console.Write('"');
                                    System.Console.Write(':');
                                    System.Console.Write('[');
                                    System.Console.Write(c.ToJson());
                                    System.Console.Write(']');
                                }
                                System.Console.WriteLine('}');
                            }

                            static partial void Run(string className)
                            {
                                GetSolver(className).Solve();
                            }

                            static CompetitiveVerifier.ProblemSolver GetSolver(string className)
                            {
                                switch(className)
                                {
                        case "HelloWorldAoj":return new HelloWorldAoj();
                        
                                    default: throw new System.ArgumentException($"{className} is not found. notice: CompetitiveVerifier require FullName as argument.", nameof(className));
                                }
                            }
                        }
                        """.ReplaceLineEndings()
        )).ToArray();

    [Fact]
    public async Task Default()
    {
        var test = new Test
        {
            TestState =
                {
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("VERIFY0002").WithSpan("/home/mine/HelloWorldAoj2.cs", 2, 1, 10, 2).WithArguments("Space.HelloWorldAoj2"),
                    },
                    OutputKind = OutputKind.ConsoleApplication,
                }
        };
        foreach (var tup in Sources) test.TestState.Sources.Add(tup);
        foreach (var tup in GeneratedSources) test.TestState.GeneratedSources.Add(tup);

        await test.RunAsync();
    }
}
