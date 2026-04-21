using System.Runtime.CompilerServices;
using VsTest.ClassLibrary;
using Assert = Xunit.Assert;

namespace VsTest.Local;

public class CircleNoArgsTest
{
    [Fact]
    public void BitEqual()
    {
        double d = 8400.2183842103;
        var c = new Circle(d);
        Assert.Equal(d, Unsafe.BitCast<Circle, double>(c));
    }
}