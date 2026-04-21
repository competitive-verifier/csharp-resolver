using Mtp.ClassLibrary;
using Assert = TUnit.Assertions.Assert;

namespace Mtp.Local.TUnit;

public class CircleTest
{
    public static (double, double)[] AreaData =>
    [
        (5, 5*5*Math.PI),
        (2, 2*2*Math.PI),
    ];

    [Test]
    [MethodDataSource(nameof(AreaData))]
    public async Task Area(double r, double expected)
    {
        await Assert.That(Math.Abs(new Circle(r).Area - expected)).IsLessThan(1e-5);
    }

    public static (double, double)[] CircumferenceData =>
    [
        (5, 2*5*Math.PI),
        (2, 2*2*Math.PI),
    ];

    [Test]
    [MethodDataSource(nameof(CircumferenceData))]
    public async Task Circumference(double r, double expected)
    {
        await Assert.That(Math.Abs(new Circle(r).Circumference - expected)).IsLessThan(1e-5);
    }
}