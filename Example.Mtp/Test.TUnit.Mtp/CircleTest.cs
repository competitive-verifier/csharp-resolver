using Mtp.ClassLibrary;
using Assert = TUnit.Assertions.Assert;

namespace Mtp.Local.TUnit;

public class MultiTests
{
    public static IEnumerable<int> Numbers => Enumerable.Range(0, 4);

    [Test]
    [MethodDataSource(nameof(Numbers), DeferEnumeration = true)]
    public async Task Defer(int number)
    {
        await Assert.That(number % 2).IsEqualTo(0);
    }

    [Test]
    [MethodDataSource(nameof(Numbers))]
    public async Task Normal(int number)
    {
        await Assert.That(number % 2).IsEqualTo(0);
    }
}
