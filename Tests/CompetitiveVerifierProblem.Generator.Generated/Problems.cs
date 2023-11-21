using CompetitiveVerifier;
internal class Aplusb : ProblemSolver
{
    public override string Url => "https://judge.yosupo.jp/problem/aplusb";
    public override void Solve()
    {
        Console.WriteLine(Console.ReadLine()!.Split().Select(int.Parse).Sum());
    }
}

internal class Pi : ProblemSolver
{
    public override string Url => "https://example.com/pi";
    public override double? Error => 1e-9;
    public override double? Tle => 12.3;
    public override void Solve()
    {
        Console.WriteLine(int.Parse(Console.ReadLine()!) * Math.PI);
    }
}