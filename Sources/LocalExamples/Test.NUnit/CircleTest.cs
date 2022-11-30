using ClassLibrary;

namespace Local.NUnit;

public class CircleTest
{
    public static object[][] AreaData => new[]
    {
        new object[] { new Circle(5), 5*5*Math.PI },
        new object[] { new Circle(2), 2*2*Math.PI },
    };

    [TestCaseSource(nameof(AreaData))]
    public void Area(Circle c, double expected)
    {
        Assert.That(Math.Abs(c.Area - expected), Is.LessThan(1e-5));
    }
    public static object[][] CircumferenceData => new[]
    {
        new object[] { new Circle(5), 2*5*Math.PI },
        new object[] { new Circle(2), 2*2*Math.PI },
    };

    [TestCaseSource(nameof(CircumferenceData))]
    public void Circumference(Circle c, double expected)
    {
        Assert.That(Math.Abs(c.Circumference - expected), Is.LessThan(1e-5));
    }
}