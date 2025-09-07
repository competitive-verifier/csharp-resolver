using ClassLibrary;
using Local.XUnit;
using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;
[assembly: RegisterXunitSerializer(typeof(CircleSerializer), typeof(Circle))]

namespace Local.XUnit;

public class CircleSerializer : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue)
        => new Circle(double.Parse(serializedValue));

    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
    {
        if (type != typeof(Circle))
        {
            failureReason = "Invalid type";
            return false;
        }
        failureReason = null;
        return true;
    }

    public string Serialize(object value) => ((Circle)value).R.ToString();

}

public class CircleTest
{
    public static TheoryData<Circle, double> AreaData = new()
    {
        { new (5), 5*5*Math.PI },
        { new (2), 2*2*Math.PI },
    };

    [Theory]
    [MemberData(nameof(AreaData))]
    public void Area(Circle c, double expected)
    {
        Assert.True(Math.Abs(c.Area - expected) < 1e-5);
    }
    public static TheoryData<Circle, double> CircumferenceData = new()
    {
        { new (5), 2*5*Math.PI },
        { new (2), 2*2*Math.PI },
    };

    [Theory]
    [MemberData(nameof(CircumferenceData))]
    public void Circumference(Circle c, double expected)
    {
        Assert.True(Math.Abs(c.Circumference - expected) < 1e-5);
    }
}