using CompetitiveVerifierCsResolver.Verifier;

namespace CompetitiveVerifierCsResolver;
internal record UnitTestResult(string Name, int Success, int Skipped, int Failure)
{
    public UnitTestResult Add(UnitTestResult other)
        => this with
        {
            Success = Success + other.Success,
            Skipped = Skipped + other.Skipped,
            Failure = Failure + other.Failure,
        };

    public IEnumerable<ConstVerification> EnumerateVerifications()
        => Enumerable.Repeat(new ConstVerification(ConstVerificationStatus.Success), Success)
        .Concat(Enumerable.Repeat(new ConstVerification(ConstVerificationStatus.Skipped), Skipped))
        .Concat(Enumerable.Repeat(new ConstVerification(ConstVerificationStatus.Failure), Failure));
}
