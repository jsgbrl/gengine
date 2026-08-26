using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers the free functions of <see cref="Vector2"/>.</summary>
public sealed class Vector2FunctionTests
{
    [Test]
    public void Dot_OfPerpendicularVectors_IsZero()
    {
        Assert.ApproximatelyEqual(0.0f, Vector2.Dot(Vector2.UnitX, Vector2.UnitY));
    }

    [Test]
    public void Dot_OfOppositeVectors_IsMinusTheProductOfTheLengths()
    {
        Assert.ApproximatelyEqual(-6.0f, Vector2.Dot(new Vector2(2.0f, 0.0f), new Vector2(-3.0f, 0.0f)));
    }

    [Test]
    public void Cross_OfParallelVectors_IsZero()
    {
        Assert.ApproximatelyEqual(0.0f, Vector2.Cross(new Vector2(2.0f, 4.0f), new Vector2(1.0f, 2.0f)));
    }

    [Test]
    public void Cross_OfTheUnitAxes_IsOne()
    {
        Assert.ApproximatelyEqual(1.0f, Vector2.Cross(Vector2.UnitX, Vector2.UnitY));
    }

    [Test]
    public void Distance_IsSymmetric_AndAgreesWithItsSquaredForm()
    {
        var from = new Vector2(1.0f, 1.0f);
        var to = new Vector2(4.0f, 5.0f);
        Assert.ApproximatelyEqual(5.0f, Vector2.Distance(from, to));
        Assert.ApproximatelyEqual(5.0f, Vector2.Distance(to, from));
        Assert.ApproximatelyEqual(25.0f, Vector2.DistanceSquared(from, to));
    }

    [TestCase(0.0f, 0.0f)]
    [TestCase(0.5f, 5.0f)]
    [TestCase(1.0f, 10.0f)]
    [TestCase(2.0f, 10.0f)]
    public void Lerp_ClampsTheAmountToTheEnds(float amount, float expectedX)
    {
        var result = Vector2.Lerp(Vector2.Zero, new Vector2(10.0f, 0.0f), amount);
        Assert.ApproximatelyEqual(expectedX, result.X);
    }

    [Test]
    public void MinAndMax_PickPerAxis_NotPerVector()
    {
        var left = new Vector2(1.0f, 9.0f);
        var right = new Vector2(5.0f, 2.0f);
        Assert.AreEqual(new Vector2(1.0f, 2.0f), Vector2.Min(left, right));
        Assert.AreEqual(new Vector2(5.0f, 9.0f), Vector2.Max(left, right));
    }

    [Test]
    public void Abs_MakesBothAxesPositive()
    {
        Assert.AreEqual(new Vector2(1.0f, 2.0f), Vector2.Abs(new Vector2(-1.0f, -2.0f)));
    }

    [Test]
    public void ClampLength_ShortensALongVectorAndKeepsItsDirection()
    {
        var clamped = Vector2.ClampLength(new Vector2(0.0f, 100.0f), 10.0f);
        Assert.ApproximatelyEqual(10.0f, clamped.Length);
        Assert.ApproximatelyEqual(10.0f, clamped.Y);
    }

    [Test]
    public void ClampLength_LeavesAShortVectorExactlyAsItWas()
    {
        var value = new Vector2(3.0f, 4.0f);
        Assert.AreEqual(value, Vector2.ClampLength(value, 10.0f));
    }
}
