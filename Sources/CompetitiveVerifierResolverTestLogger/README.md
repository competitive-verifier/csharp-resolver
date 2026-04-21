# CompetitiveVerifierResolverTestLogger

## Get started

```sh
dotnet add package CompetitiveVerifierResolverTestLogger
```

## Usage

For vstest project

```sh
dotnet test --logger "CompetitiveVerifier;OutDirectory=$PWD/VerifierUnitTest"
```

For Microsoft.Testing.Platform project

```sh
dotnet test --report-competitive-verifier "$PWD/VerifierUnitTest"
```