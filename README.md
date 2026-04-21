# csharp-resolver

|Library|NuGet|
|:---|:---|
|CompetitiveVerifierCsResolver|[![NuGet version (CompetitiveVerifierCsResolver)](https://img.shields.io/nuget/v/CompetitiveVerifierCsResolver.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierCsResolver/)|
|CompetitiveVerifierProblem|[![NuGet version (CompetitiveVerifierProblem)](https://img.shields.io/nuget/v/CompetitiveVerifierProblem.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierProblem/)|
|CompetitiveVerifierResolverTestLogger|[![NuGet version (CompetitiveVerifierResolverTestLogger)](https://img.shields.io/nuget/v/CompetitiveVerifierResolverTestLogger.svg?style=flat-square)](https://www.nuget.org/packages/CompetitiveVerifierResolverTestLogger/)|


See [Example.VsTest](Example.VsTest).

## Usage

Use in GitHub Actions.

**Sample**

```yaml
      - name: Build
        run: dotnet build ${{ env.WORKFLOW_BUILD_SLN }} -c Release
      - name: setup CompetitiveVerifierCsResolver
        run: dotnet tool install -g CompetitiveVerifierCsResolver
      - name: Unit test
        run: dotnet test ${{ env.UNITTEST_CSPROJ }} --logger "CompetitiveVerifier;OutDirectory=${{runner.temp}}/VerifierUnitTest" --no-build  -c Release
      - name: Resolve
        run: dotnet run --project ${{ env.VERIFY_CSPROJ }} --no-build -c Release | tee ${{runner.temp}}/problems.json
      - name: cs-resolve
        uses: competitive-verifier/actions/cs-resolve@v2
        with:
          solution: ${{ env.WORKFLOW_BUILD_SLN }}
          output-path: verify_files.json
          # Specify patterns
          include: |
                Example.VsTest/**
          # exclude: your-own-exclude/
          unittest-result: ${{runner.temp}}/VerifierUnitTest/*.csv
          problems: ${{runner.temp}}/problems.json
```

### Local

```sh
# Install
dotnet tool install -g CompetitiveVerifierCsResolver
dotnet add {{YourUnittest.csproj}} package CompetitiveVerifierResolverTestLogger
dotnet add {{YourProblemApp.csproj}} package CompetitiveVerifierProblem

# Run
dotnet test {{YourUnittest.csproj}} --logger "CompetitiveVerifier;OutFile=$pwd/VerifierUnitTest"
dotnet run --project {{YourProblemApp.csproj}} > problems.json
CompetitiveVerifierCsResolver YourSolution.sln -u VerifierUnitTest/*.csv -p problems.json
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

See [Example.VsTest/VerifyApp](Example.VsTest/VerifyApp).

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
dotnet test --logger "CompetitiveVerifier;OutDirectory=$PWD/VerifierUnitTest"
```

For Microsoft.Testing.Platform project.

```sh
dotnet test --solution {your.slnx} --report-competitive-verifier "$PWD/VerifierUnitTest"
```

NUnit is not yet supported in the MTP version. https://github.com/nunit/nunit3-vs-adapter/issues/1351