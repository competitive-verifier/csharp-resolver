# csharp-resolver

|Library|NuGet|
|:---|:---|
|CompetitiveCsResolver|[![NuGet version (CompetitiveCsResolver)](https://img.shields.io/nuget/v/CompetitiveCsResolver.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveCsResolver/)|
|CompetitiveVerifierProblem|[![NuGet version (CompetitiveVerifierProblem)](https://img.shields.io/nuget/v/CompetitiveVerifierProblem.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierProblem/)|
|CompetitiveVerifierProblem.Generator|[![NuGet version (CompetitiveVerifierProblem.Generator)](https://img.shields.io/nuget/v/CompetitiveVerifierProblem.Generator.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierProblem.Generator/)|
|CompetitiveVerifierResolverTestLogger|[![NuGet version (CompetitiveVerifierResolverTestLogger)](https://img.shields.io/nuget/v/CompetitiveVerifierResolverTestLogger.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierResolverTestLogger/)|


See [Examples](Examples).

## Usage

Use in GitHub Actions.

TODO: 書く

## Projects
### CompetitiveCsResolver
Resolve solution dependency and verifications.

### CompetitiveVerifierProblem

Subclasses of `CompetitiveVerifier.ProblemSolver` are treated as verification.

```cs
using ClassLibrary;
using System;

namespace VerifyApp;

// competitive-verifier: document_title Hello World! test
internal class CircleAizu : CompetitiveVerifier.ProblemSolver
{
    public override string Url => "http://judge.u-aizu.ac.jp/onlinejudge/description.jsp?id=ITP1_4_B";
    public override double? Error => 1e-5;
    public override void Solve()
    {
        var r = double.Parse(Console.ReadLine()!);
        var circle = new Circle(r);
        Console.WriteLine($"{circle.Area} {circle.Circumference}");
    }
}
```

### CompetitiveVerifierProblem.Generator

Implicit usings.

### CompetitiveVerifierResolverTestLogger

Add unit test project.
Run with OutFile parameter.

```sh
dotnet test  --logger "CompetitiveVerifier;OutFile=$pwd/out.csv"
```