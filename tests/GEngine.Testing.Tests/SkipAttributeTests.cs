namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="SkipAttribute"/>.</summary>
public sealed class SkipAttributeTests
{
    [Test]
    public void Reason_IsKeptAndReportedVerbatim()
    {
        Assert.AreEqual("no joystick", new SkipAttribute("no joystick").Reason);
    }
}
