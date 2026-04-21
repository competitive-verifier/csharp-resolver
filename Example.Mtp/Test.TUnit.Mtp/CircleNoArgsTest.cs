using Mtp.ClassLibrary;
using System.Runtime.CompilerServices;
using Assert = TUnit.Assertions.Assert;

namespace Mtp.Local;

public class CircleNoArgsTest
{
    [Test]
    public async Task BitEqual()
    {
        double d = 8400.2183842103;
        var c = new Circle(d);
        await Assert.That(Unsafe.BitCast<Circle, double>(c)).EqualTo(d);
    }
}