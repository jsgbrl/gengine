using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="MathG"/>.</summary>
public sealed class MathGTests
{
    [Test]
    public void Approximately_AcceptsErrorThatExactEqualityRejects()
    {
        float accumulated = SumOfTenTenths();
        Assert.IsFalse(accumulated.Equals(1.0f), "ten tenths do not add up to one in single precision");
        Assert.IsTrue(MathG.Approximately(accumulated, 1.0f));
    }

    [Test]
    public void Approximately_StillRejectsARealDifference()
    {
        Assert.IsFalse(MathG.Approximately(1.0f, 1.001f));
    }

    private static float SumOfTenTenths()
    {
        float total = 0.0f;
        for (int step = 0; step < 10; step++)
        {
            total += 0.1f;
        }

        return total;
    }

    [TestCase(-5.0f, 0.0f)]
    [TestCase(0.5f, 0.5f)]
    [TestCase(5.0f, 1.0f)]
    public void Clamp01_PullsAValueToTheNearestEnd(float value, float expected)
    {
        Assert.ApproximatelyEqual(expected, MathG.Clamp01(value));
    }

    [Test]
    public void Clamp_WorksForAnyRange()
    {
        Assert.ApproximatelyEqual(3.0f, MathG.Clamp(1.0f, 3.0f, 7.0f));
        Assert.ApproximatelyEqual(7.0f, MathG.Clamp(9.0f, 3.0f, 7.0f));
    }

    [Test]
    public void Lerp_ClampsWhereLerpUnclampedOvershoots()
    {
        Assert.ApproximatelyEqual(10.0f, MathG.Lerp(0.0f, 10.0f, 2.0f));
        Assert.ApproximatelyEqual(20.0f, MathG.LerpUnclamped(0.0f, 10.0f, 2.0f));
    }

    [Test]
    public void MoveTowards_NeverOvershootsTheTarget()
    {
        Assert.ApproximatelyEqual(10.0f, MathG.MoveTowards(0.0f, 10.0f, 100.0f));
        Assert.ApproximatelyEqual(1.0f, MathG.MoveTowards(0.0f, 10.0f, 1.0f));
        Assert.ApproximatelyEqual(-1.0f, MathG.MoveTowards(0.0f, -10.0f, 1.0f));
    }

    [TestCase(-3.0f, -1.0f)]
    [TestCase(0.0f, 0.0f)]
    [TestCase(3.0f, 1.0f)]
    public void Sign_MapsZeroToZero(float value, float expected)
    {
        Assert.ApproximatelyEqual(expected, MathG.Sign(value));
    }

    [Test]
    public void Rounding_HelpersAgreeWithTheirNames()
    {
        Assert.AreEqual(-3, MathG.FloorToInt(-2.5f));
        Assert.AreEqual(-2, MathG.CeilToInt(-2.5f));
        Assert.AreEqual(3, MathG.RoundToInt(2.5f));
        Assert.AreEqual(-3, MathG.RoundToInt(-2.5f));
    }
}
