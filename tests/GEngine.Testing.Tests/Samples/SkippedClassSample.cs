// A skip on the class covers every test in it.

namespace GEngine.Testing.Tests.Samples;

[Skip(SkippedClassSample.Reason)]
internal sealed class SkippedClassSample
{
    internal const string Reason = "the whole suite needs hardware";

    [Test]
    public void NeverRuns()
    {
        Assert.Fail("this body must never execute");
    }

    [Test]
    public void AlsoNeverRuns()
    {
        Assert.Fail("this body must never execute either");
    }
}
