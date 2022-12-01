# csharp-resolver

|Library|NuGet|
|:---|:---|
|CompetitiveVerifierCsResolver|[![NuGet version (CompetitiveVerifierCsResolver)](https://img.shields.io/nuget/v/CompetitiveVerifierCsResolver.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierCsResolver/)|
|CompetitiveVerifierProblem|[![NuGet version (CompetitiveVerifierProblem)](https://img.shields.io/nuget/v/CompetitiveVerifierProblem.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierProblem/)|
|CompetitiveVerifierProblem.Generator|[![NuGet version (CompetitiveVerifierProblem.Generator)](https://img.shields.io/nuget/v/CompetitiveVerifierProblem.Generator.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierProblem.Generator/)|
|CompetitiveVerifierResolverTestLogger|[![NuGet version (CompetitiveVerifierResolverTestLogger)](https://img.shields.io/nuget/v/CompetitiveVerifierResolverTestLogger.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierResolverTestLogger/)|


See [Examples](Examples).

## Usage

Use in GitHub Actions.

TODO: 書く

### Local

```sh
# Install
dotnet tool install -g CompetitiveVerifierCsResolver
dotnet add {{YourUnittest.csproj}} package CompetitiveVerifierResolverTestLogger
dotnet add {{YourProblemApp.csproj}} package CompetitiveVerifierProblem

# Run
dotnet test {{YourUnittest.csproj}} --logger "CompetitiveVerifier;OutFile=$pwd/unittest.csv"
dotnet run --project {{YourProblemApp.csproj}} > problems.json
CompetitiveVerifierCsResolver YourSolution.sln -u unittest.csv -p problems.json
```

## Projects
### CompetitiveVerifierCsResolver
Resolve solution dependency and verifications.

```sh
dotnet tool install -g CompetitiveVerifierCsResolver
```

### CompetitiveVerifierProblem

Subclasses of `CompetitiveVerifier.ProblemSolver` are treated as verification.

The library assume for console application.

See [Examples/VerifyApp](Examples/VerifyApp).

#### Example: Problem

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