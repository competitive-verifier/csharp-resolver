using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;

namespace CompetitiveVerifierProblem.Generator.Test;

public class GenericSolverTest : TestBase
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
    public override string Url => "https://onlinejudge.u-aizu.ac.jp/courses/lesson/2/ITP1/1/ITP1_1_A";
    public override void Solve()
    {
        global::System.Console.WriteLine("Hello World");
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
internal abstract class WithoutTypeArgumentsBase<T> : CompetitiveVerifier.ProblemSolver where T : IUrl, new()
{
    public override string Url => new T().Url;

    public override void Solve() { }
}
internal class WithoutTypeArguments<T> : WithoutTypeArgumentsBase<T> where T : IUrl, new()
{
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
                        new Space.HelloWorldAoj2(),
                        new Space.WithTypeArguments(),

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
                        case "Space.HelloWorldAoj2":return new Space.HelloWorldAoj2();
                        case "Space.WithTypeArguments":return new Space.WithTypeArguments();
                        case "HelloWorldAoj2":return new Space.HelloWorldAoj2();
                        case "WithTypeArguments":return new Space.WithTypeArguments();
                        
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
                            .WithSpan("/home/mine/WithoutConstructorGeneric.cs", 6, 16, 6, 41)
                            .WithArguments("Space.WithoutConstructorGeneric"),
                        DiagnosticResult.CompilerWarning("VERIFY0003")
                            .WithSpan("/home/mine/WithoutTypeArguments.cs", 8, 16, 8, 36),
                    },
                    OutputKind = OutputKind.ConsoleApplication,
                }
        };
        foreach (var tup in Sources) test.TestState.Sources.Add(tup);
        foreach (var tup in GeneratedSources) test.TestState.GeneratedSources.Add(tup);

        await test.RunAsync(cancellationToken);
    }
}