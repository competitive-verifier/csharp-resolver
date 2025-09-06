using ClassLibrary;

namespace Local.MSTest;

[TestClass]
public class CircleTest
{
    public static object[][] AreaData() => new object[][]
    {
        new object[] { 5, 5*5*Math.PI },
        new object[] { 2, 2*2*Math.PI },
    };

    [TestMethod]
    [DynamicData(nameof(AreaData), DynamicDataSourceType.Method)]
    public void Area(int r, double expected)
    {
        var c = new Circle(r);
        Assert.IsTrue(Math.Abs(c.Area - expected) < 1e-5);
    }
    public static object[][] CircumferenceData() => new object[][]
    {
        new object[] { 5, 2*5*Math.PI },
        new object[] { 2, 2*2*Math.PI },
    };

    [TestMethod]
    [DynamicData(nameof(CircumferenceData), DynamicDataSourceType.Method)]
    public void Circumference(int r, double expected)
    {
        var c = new Circle(r);
        Assert.IsTrue(Math.Abs(c.Circumference - expected) < 1e-5);
    }
}