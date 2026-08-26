// A sample whose assertion is meant to fail, so the runner's failure path has something
// to report.

namespace GEngine.Testing.Tests.Samples;

internal sealed class FailingSample
{
    internal const string ExpectedMessage = "the wheels came off";

    [Test]
    public void AlwaysFails()
    {
        Assert.Fail(ExpectedMessage);
    }
}
