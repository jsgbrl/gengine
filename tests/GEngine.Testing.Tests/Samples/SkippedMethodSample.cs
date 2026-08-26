// One skipped method next to one that runs.

namespace GEngine.Testing.Tests.Samples;

internal sealed class SkippedMethodSample
{
    internal const string Reason = "needs a DualSense plugged in";

    [Test]
    [Skip(Reason)]
    public void NeverRuns()
    {
        Assert.Fail("this body must never execute");
    }

    [Test]
    public void StillRuns()
    {
        Assert.IsTrue(true);
    }
}
