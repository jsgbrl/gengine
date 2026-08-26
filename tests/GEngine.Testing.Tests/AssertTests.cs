using System;

namespace GEngine.Testing.Tests;

/// <summary>Covers the general assertions of <see cref="Assert"/>.</summary>
public sealed class AssertTests
{
    [Test]
    public void IsTrue_WhenConditionHolds_DoesNotThrow()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void IsTrue_WhenConditionFails_ThrowsWithBothSides()
    {
        AssertionException failure = Assert.Throws<AssertionException>(static () => Assert.IsTrue(false));
        Assert.IsTrue(failure.Message.Contains("expected: true", StringComparison.Ordinal));
        Assert.IsTrue(failure.Message.Contains("actual:   false", StringComparison.Ordinal));
    }

    [Test]
    public void IsFalse_WhenConditionHolds_Throws()
    {
        Assert.Throws<AssertionException>(static () => Assert.IsFalse(true));
    }

    [Test]
    public void AreEqual_ComparesByValue_NotByReference()
    {
        Assert.AreEqual("ab", string.Concat("a", "b"));
    }

    [Test]
    public void AreEqual_WhenDifferent_NamesTheContextFirst()
    {
        AssertionException failure = Assert.Throws<AssertionException>(
            static () => Assert.AreEqual(1, 2, "counting"));
        Assert.IsTrue(failure.Message.StartsWith("counting", StringComparison.Ordinal));
    }

    [Test]
    public void AreNotEqual_WhenEqual_Throws()
    {
        Assert.Throws<AssertionException>(static () => Assert.AreNotEqual(7, 7));
    }

    [Test]
    public void AreSame_DistinguishesTwoEqualButDistinctInstances()
    {
        object first = new StringBuilderLike();
        object second = new StringBuilderLike();
        Assert.Throws<AssertionException>(() => Assert.AreSame(first, second));
        Assert.AreSame(first, first);
    }

    [Test]
    public void IsNull_AndIsNotNull_AreOpposites()
    {
        Assert.IsNull(null);
        Assert.IsNotNull("something");
        Assert.Throws<AssertionException>(static () => Assert.IsNull("something"));
        Assert.Throws<AssertionException>(static () => Assert.IsNotNull(null));
    }

    [Test]
    public void Throws_ReturnsTheExceptionSoTheTestCanInspectIt()
    {
        InvalidOperationException thrown = Assert.Throws<InvalidOperationException>(
            static () => throw new InvalidOperationException("boom"));
        Assert.AreEqual("boom", thrown.Message);
    }

    [Test]
    public void Throws_WhenNothingThrows_Fails()
    {
        AssertionException failure = Assert.Throws<AssertionException>(
            static () => Assert.Throws<InvalidOperationException>(static () => { }));
        Assert.IsTrue(failure.Message.Contains("no exception", StringComparison.Ordinal));
    }

    [Test]
    public void Fail_AlwaysThrowsWithTheGivenMessage()
    {
        AssertionException failure = Assert.Throws<AssertionException>(static () => Assert.Fail("stop"));
        Assert.AreEqual("stop", failure.Message);
    }

    private sealed class StringBuilderLike
    {
    }
}
