namespace GEngine.Testing.Tests;

/// <summary>Covers the floating-point assertions of <see cref="Assert"/>.</summary>
public sealed class AssertNumericTests
{
    [Test]
    public void ApproximatelyEqual_AcceptsADifferenceInsideTheTolerance()
    {
        Assert.ApproximatelyEqual(1.0f, 1.0f + (Assert.DefaultTolerance / 2.0f));
    }

    [Test]
    public void ApproximatelyEqual_RejectsADifferenceOutsideTheTolerance()
    {
        Assert.Throws<AssertionException>(static () => Assert.ApproximatelyEqual(1.0f, 1.1f));
    }

    [Test]
    public void ApproximatelyEqual_RejectsNaN_BecauseEveryComparisonWithItIsFalse()
    {
        Assert.Throws<AssertionException>(static () => Assert.ApproximatelyEqual(float.NaN, float.NaN));
    }

    [TestCase(0.0f)]
    [TestCase(0.5f)]
    [TestCase(1.0f)]
    public void IsInRange_AcceptsBothEndsAndTheMiddle(float value)
    {
        Assert.IsInRange(value, 0.0f, 1.0f);
    }

    [TestCase(-0.001f)]
    [TestCase(1.001f)]
    public void IsInRange_RejectsJustOutside(float value)
    {
        Assert.Throws<AssertionException>(() => Assert.IsInRange(value, 0.0f, 1.0f));
    }

    [Test]
    public void IsFinite_RejectsNaNAndInfinity()
    {
        Assert.IsFinite(0.0f);
        Assert.Throws<AssertionException>(static () => Assert.IsFinite(float.NaN));
        Assert.Throws<AssertionException>(static () => Assert.IsFinite(float.PositiveInfinity));
    }
}
