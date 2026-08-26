namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="TestResult"/>.</summary>
public sealed class TestResultTests
{
    [Test]
    public void Constructor_KeepsEveryFieldItWasGiven()
    {
        var result = new TestResult("Suite.Case", TestOutcome.Failed, 12.5, "why");
        Assert.AreEqual("Suite.Case", result.Name);
        Assert.AreEqual(TestOutcome.Failed, result.Outcome);
        Assert.ApproximatelyEqual(12.5f, (float)result.ElapsedMilliseconds);
        Assert.AreEqual("why", result.Message);
    }
}
