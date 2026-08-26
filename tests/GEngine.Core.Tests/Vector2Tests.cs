using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="Vector2"/>, edge cases first.</summary>
public sealed class Vector2Tests
{
    [Test]
    public void Length_OfUnitX_IsOne()
    {
        Assert.ApproximatelyEqual(1.0f, Vector2.UnitX.Length);
    }

    [Test]
    public void Length_OfThreeFour_IsFive()
    {
        Assert.ApproximatelyEqual(5.0f, new Vector2(3.0f, 4.0f).Length);
    }

    [Test]
    public void LengthSquared_SkipsTheSquareRoot_AndAgreesWithLength()
    {
        var value = new Vector2(3.0f, 4.0f);
        Assert.ApproximatelyEqual(value.Length * value.Length, value.LengthSquared, 1e-4f);
    }

    [Test]
    public void Normalized_OfZero_ReturnsZero_NotNaN()
    {
        Vector2 normalized = Vector2.Zero.Normalized();
        Assert.AreEqual(Vector2.Zero, normalized);
        Assert.IsFinite(normalized.X);
        Assert.IsFinite(normalized.Y);
    }

    [Test]
    public void Normalized_KeepsDirectionAndSetsLengthToOne()
    {
        Vector2 normalized = new Vector2(0.0f, -8.0f).Normalized();
        Assert.ApproximatelyEqual(1.0f, normalized.Length);
        Assert.ApproximatelyEqual(-1.0f, normalized.Y);
    }

    [Test]
    public void Normalized_OfAVectorShorterThanEpsilon_IsAlsoZero()
    {
        Assert.AreEqual(Vector2.Zero, new Vector2(1e-9f, 0.0f).Normalized());
    }

    [Test]
    public void WithX_AndWithY_ChangeOneAxisAndLeaveTheOther()
    {
        var value = new Vector2(1.0f, 2.0f);
        Assert.AreEqual(new Vector2(9.0f, 2.0f), value.WithX(9.0f));
        Assert.AreEqual(new Vector2(1.0f, 9.0f), value.WithY(9.0f));
    }

    [Test]
    public void ApproximatelyEquals_ToleratesAccumulatedError_WhereEqualsDoesNot()
    {
        Vector2 accumulated = Vector2.Zero;
        for (int step = 0; step < 10; step++)
        {
            accumulated += new Vector2(0.1f, 0.0f);
        }

        var exact = new Vector2(1.0f, 0.0f);
        Assert.IsTrue(accumulated.ApproximatelyEquals(exact));
        Assert.IsFalse(accumulated.Equals(exact), "ten tenths do not add up to one in single precision");
    }

    [Test]
    public void Equals_AndOperators_AgreeWithEachOther()
    {
        var left = new Vector2(1.0f, 2.0f);
        var right = new Vector2(1.0f, 2.0f);
        Assert.IsTrue(left == right);
        Assert.IsFalse(left != right);
        Assert.IsTrue(left.Equals((object)right));
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    [Test]
    public void Equals_AgainstAnotherType_IsFalse()
    {
        Assert.IsFalse(Vector2.Zero.Equals("not a vector"));
    }

    [Test]
    public void ToString_ShowsBothAxes_WithAnInvariantDecimalPoint()
    {
        Assert.AreEqual("(1.5, -2)", new Vector2(1.5f, -2.0f).ToString());
    }
}
