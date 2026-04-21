namespace VerifyApp;

internal class HelloWorldAoj : CompetitiveVerifier.ProblemSolver
{
    public override string Url => "https://onlinejudge.u-aizu.ac.jp/courses/lesson/2/ITP1/1/ITP1_1_A";
    public override void Solve()
    {
        ClassLibrary.HelloWorld.Say();
    }
}
