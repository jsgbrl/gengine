using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers the operators of <see cref="Vector2"/> and their named twins.</summary>
public sealed class Vector2OperatorTests
{
    [Test]
    public void Addition_AndSubtraction_WorkComponentWise()
    {
        var left = new Vector2(1.0f, 2.0f);
        var right = new Vector2(10.0f, 20.0f);
        Assert.AreEqual(new Vector2(11.0f, 22.0f), left + right);
        Assert.AreEqual(new Vector2(-9.0f, -18.0f), left - right);
    }

    [Test]
    public void Negation_ReversesBothAxes()
    {
        Assert.AreEqual(new Vector2(-1.0f, 2.0f), -new Vector2(1.0f, -2.0f));
    }

    [Test]
    public void Scaling_ReadsTheSameFromEitherSide()
    {
        var value = new Vector2(1.0f, -2.0f);
        Assert.AreEqual(new Vector2(3.0f, -6.0f), value * 3.0f);
        Assert.AreEqual(new Vector2(3.0f, -6.0f), 3.0f * value);
    }

    [Test]
    public void Division_IsScalingByTheReciprocal()
    {
        Assert.AreEqual(new Vector2(2.0f, -1.0f), new Vector2(4.0f, -2.0f) / 2.0f);
    }

    [Test]
    public void NamedMethods_MatchTheOperatorsTheyBackFor()
    {
        var left = new Vector2(4.0f, 6.0f);
        var right = new Vector2(1.0f, 2.0f);
        Assert.AreEqual(left + right, Vector2.Add(left, right));
        Assert.AreEqual(left - right, Vector2.Subtract(left, right));
        Assert.AreEqual(-left, Vector2.Negate(left));
        Assert.AreEqual(left * 2.0f, Vector2.Multiply(left, 2.0f));
        Assert.AreEqual(left / 2.0f, Vector2.Divide(left, 2.0f));
    }
}
