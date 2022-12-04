using ClassLibrary;
using System;

namespace VerifyApp;

internal class CircleAizuExt : AbstractSolver
{
    public override string Url => "http://judge.u-aizu.ac.jp/onlinejudge/description.jsp?id=ITP1_4_B";
    public override double? Error => 1e-5;
    public override void Solve()
    {
        var r = double.Parse(Console.ReadLine()!);
        Console.WriteLine($"{r.Area()} {r.Circumference()}");
    }
}