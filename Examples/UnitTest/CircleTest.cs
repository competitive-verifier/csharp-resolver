using ClassLibrary;

namespace UnitTest;

public class CircleTest
{
    public static TheoryData<Circle, double> AreaData = new TheoryData<Circle, double>
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
    public static TheoryData<Circle, double> CircumferenceData = new TheoryData<Circle, double>
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