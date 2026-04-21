using ClassLibrary;

namespace Local.MSTest;

[TestClass]
public class CircleTest
{
    public static object[][] AreaData() =>
    [
        [5, 5*5*Math.PI],
        [2, 2*2*Math.PI],
    ];

    [TestMethod]
    [DynamicData(nameof(AreaData))]
    public void Area(int r, double expected)
    {
        var c = new Circle(r);
        Assert.IsLessThan(1e-5, Math.Abs(c.Area - expected));
    }
    public static object[][] CircumferenceData() =>
    [
        [5, 2*5*Math.PI],
        [2, 2*2*Math.PI],
    ];

    [TestMethod]
    [DynamicData(nameof(CircumferenceData))]
    public void Circumference(int r, double expected)
    {
        var c = new Circle(r);
        Assert.IsLessThan(1e-5, Math.Abs(c.Circumference - expected));
    }
}