using System.Text.Json.Serialization;

namespace CompetitiveVerifierCsResolver.Verifier;

[JsonSerializable(typeof(VerificationInput))]
[JsonSerializable(typeof(ConstVerification))]
[JsonSerializable(typeof(ProblemVerification))]
[JsonSerializable(typeof(VerificationFile))]
[JsonSerializable(typeof(Dictionary<string, ProblemVerification[]>))]
internal partial class VerificationJsonContext : JsonSerializerContext
{
    public static VerificationJsonContext IgnoreNull { get; } = new(new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
}
