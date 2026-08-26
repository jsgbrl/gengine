namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="TestCaseAttribute"/>.</summary>
public sealed class TestCaseAttributeTests
{
    [Test]
    public void Arguments_KeepDeclarationOrder()
    {
        var attribute = new TestCaseAttribute(1, "two", 3.0f);
        Assert.AreEqual(3, attribute.Arguments.Count);
        Assert.AreEqual(1, attribute.Arguments[0]);
        Assert.AreEqual("two", attribute.Arguments[1]);
    }

    [Test]
    public void Arguments_AreEmptyWhenNoneAreGiven()
    {
        Assert.AreEqual(0, new TestCaseAttribute().Arguments.Count);
    }
}
