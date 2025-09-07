using CompetitiveVerifierCsResolver.Models;
using CompetitiveVerifierCsResolver.Verifier;
using CompetitiveVerifierProblem.Generator.Test.Serializer;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(XunitSerializer), typeof(UnitTestResult), typeof(VerificationFile))]

namespace CompetitiveVerifierProblem.Generator.Test.Serializer;

internal class XunitSerializer : IXunitSerializer
{
    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
    {
        failureReason = null;
        if (type == typeof(UnitTestResult)) return true;
        if (type.IsAssignableTo(typeof(Verification)) || type == typeof(VerificationFile)) return true;
        failureReason = "Invalid type";
        return false;
    }

    public object Deserialize(Type type, string serializedValue)
    {
        if (type == typeof(UnitTestResult))
        {
            var sp = serializedValue.Split(':');
            return new UnitTestResult(sp[0], int.Parse(sp[1]), int.Parse(sp[2]), int.Parse(sp[3]));
        }
        if (type.IsAssignableTo(typeof(Verification)) || type == typeof(VerificationFile))
        {
            return JsonSerializer.Deserialize(serializedValue, VerificationJsonContext.IgnoreNull.VerificationInput)!;
        }
        throw new NotSupportedException();
    }

    public string Serialize(object value)
    {
        if (value is UnitTestResult unitTestResult)
        {
            return $"{unitTestResult.Name}:{unitTestResult.Success}:{unitTestResult.Skipped}:{unitTestResult.Failure}";
        }
        if (value is Verification or VerificationFile)
        {
            return JsonSerializer.Serialize(value, VerificationJsonContext.IgnoreNull.VerificationInput);
        }
        throw new NotSupportedException();
    }
}
