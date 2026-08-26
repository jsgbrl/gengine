// Proves both halves of the setup contract: it runs before each test, and it runs on a
// fresh instance, so one test cannot see what the previous one left behind.

namespace GEngine.Testing.Tests.Samples;

internal sealed class SetupSample
{
    private int _counter;

    [Setup]
    public void Setup()
    {
        _counter++;
    }

    [Test]
    public void FirstSeesExactlyOneSetup()
    {
        Assert.AreEqual(1, _counter);
    }

    [Test]
    public void SecondAlsoSeesExactlyOneSetup()
    {
        Assert.AreEqual(1, _counter);
    }
}
