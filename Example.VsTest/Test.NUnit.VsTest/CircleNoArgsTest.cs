using System.Runtime.CompilerServices;
using VsTest.ClassLibrary;
using Assert = NUnit.Framework.Assert;

namespace VsTest.Local;

public class CircleNoArgsTest
{
    [Test]
    public void BitEqual()
    {
        double d = 8400.2183842103;
        var c = new Circle(d);
        Assert.AreEqual(d, Unsafe.BitCast<Circle, double>(c));
    }
}