using Assert = Xunit.Assert;

namespace Mtp.Local.XUnit;

public class MultiTests
{
    public static TheoryData<int> Numbers => new(Enumerable.Range(0, 4));

    [Theory]
    [MemberData(nameof(Numbers), DisableDiscoveryEnumeration = true)]
    public void Defer(int number)
    {
        Assert.Equal(0, number % 2);
    }

    [Theory]
    [MemberData(nameof(Numbers))]
    public void Normal(int number)
    {
        Assert.Equal(0, number % 2);
    }
}