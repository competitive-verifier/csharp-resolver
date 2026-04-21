using System.Runtime.CompilerServices;
using Mtp.ClassLibrary;

namespace Mtp.Local;

[TestClass]
public class CircleNoArgsTest
{
    [TestMethod]
    public void BitEqual()
    {
        double d = 8400.2183842103;
        var c = new Circle(d);
        Assert.AreEqual(d, Unsafe.BitCast<Circle, double>(c));
    }
}