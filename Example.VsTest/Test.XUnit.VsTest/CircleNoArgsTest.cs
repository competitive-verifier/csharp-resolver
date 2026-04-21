using System.Runtime.CompilerServices;
using ClassLibrary;
using Assert = Xunit.Assert;

namespace Local;

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