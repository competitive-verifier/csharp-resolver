using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace CompetitiveVerifierProblem.Generator.Test;
public class GenericSolverTest : TestBase
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
    public override string Url => "https://onlinejudge.u-aizu.ac.jp/courses/lesson/2/ITP1/1/ITP1_1_A";
    public override void Solve()
    {
        System.Console.WriteLine("Hello World");
    }
}
}
"""
                        ),
                        (
                            @"/home/mine/WithoutConstructorGeneric.cs",
                            """
namespace Space{
interface IUrl
{
    string Url { get; }
}
internal class WithoutConstructorGeneric<T> : CompetitiveVerifier.ProblemSolver where T : IUrl, new()
{
    public override string Url => new T().Url;

    public override void Solve() { }
    public WithoutConstructorGeneric(int v) { }
}
}
"""
                        ),
                        (
                            @"/home/mine/WithoutTypeArguments.cs",
                            """
namespace Space{
internal class WithoutTypeArguments<T> : CompetitiveVerifier.ProblemSolver where T : IUrl, new()
{
    public override string Url => new T().Url;

    public override void Solve() { }
}
}
"""
                        ),
                        (
                            @"/home/mine/WithTypeArguments.cs",
                            """
namespace Space{

class U : IUrl {
    string IUrl.Url => "dummy";
}

internal class WithTypeArguments : WithoutTypeArguments<U>
{
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
                        new Space.HelloWorldAoj2(),
                        new Space.WithTypeArguments(),

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
                        case "Space.HelloWorldAoj2":return new Space.HelloWorldAoj2();
                        case "Space.WithTypeArguments":return new Space.WithTypeArguments();
                        case "HelloWorldAoj2":return new Space.HelloWorldAoj2();
                        case "WithTypeArguments":return new Space.WithTypeArguments();
                        
                                    default: throw new System.ArgumentException($"{className} is not found.", nameof(className));
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
                        DiagnosticResult.CompilerWarning("VERIFY0002")
                            .WithSpan("/home/mine/WithoutConstructorGeneric.cs", 6, 16, 6, 41)
                            .WithArguments("Space.WithoutConstructorGeneric"),
                        DiagnosticResult.CompilerWarning("VERIFY0003")
                            .WithSpan("/home/mine/WithoutTypeArguments.cs", 2, 16, 2, 36),
                    },
                    OutputKind = OutputKind.ConsoleApplication,
                }
        };
        foreach (var tup in Sources) test.TestState.Sources.Add(tup);
        foreach (var tup in GeneratedSources) test.TestState.GeneratedSources.Add(tup);

        await test.RunAsync();
    }
}