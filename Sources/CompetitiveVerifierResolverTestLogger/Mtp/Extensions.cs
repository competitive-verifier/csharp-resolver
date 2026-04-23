using Microsoft.Testing.Platform.Extensions.Messages;

namespace CompetitiveVerifierResolverTestLogger.Mtp;

internal static class Extensions
{
    extension(TestNode test)
    {
        public string? TryGetClassName()
        {
            return test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() switch
            {
                { Namespace: null or "", TypeName: var t } => t,
                { } method => $"{method.Namespace}.{method.TypeName}",
                _ => null,
            };
        }
    }
}
