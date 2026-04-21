using ClassLibrary;
using Assert = Xunit.Assert;

namespace Local.XUnit;

public class CircleTest
{
    public static TheoryData<double, double> AreaData = new()
    {
        { 5, 5*5*Math.PI },
        { 2, 2*2*Math.PI },
    };

    [Theory]
    [MemberData(nameof(AreaData))]
    public void Area(double r, double expected)
    {
        Assert.True(Math.Abs(new Circle(r).Area - expected) < 1e-5);
    }

    public static TheoryData<double, double> CircumferenceData = new()
    {
        { 5, 2*5*Math.PI },
        { 2, 2*2*Math.PI },
    };

    [Theory]
    [MemberData(nameof(CircumferenceData))]
    public void Circumference(double r, double expected)
    {
        Assert.True(Math.Abs(new Circle(r).Circumference - expected) < 1e-5);
    }
}