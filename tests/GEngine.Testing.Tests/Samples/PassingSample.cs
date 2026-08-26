// Sample suites for the framework's own tests. They are internal on purpose: the real
// run scans exported types only, so a sample that fails here cannot fail the real suite.

namespace GEngine.Testing.Tests.Samples;

internal sealed class PassingSample
{
    [Test]
    public void AlwaysPasses()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void AlsoPasses()
    {
        Assert.AreEqual(2, 1 + 1);
    }
}
