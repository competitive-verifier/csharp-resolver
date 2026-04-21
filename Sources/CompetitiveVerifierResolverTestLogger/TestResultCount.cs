using System;

namespace CompetitiveVerifierResolverTestLogger;

internal record struct TestResultCount(string ClassName, int Success, int Skipped, int Failure)
{
    public void Increment(Outcome outcome)
    {
        switch (outcome)
        {
            case Outcome.Success:
                Success++;
                break;
            case Outcome.Skipped:
                Skipped++;
                break;
            case Outcome.Failure:
                Failure++;
                break;
            default:
                throw new InvalidOperationException($"Invalid test outcome: {outcome}");
        }
    }
}
internal enum Outcome
{
    Success,
    Skipped,
    Failure,
}