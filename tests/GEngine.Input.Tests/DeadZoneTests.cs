using GEngine.Core;
using GEngine.Input.Gamepad;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="DeadZone"/>, radial rather than per axis.</summary>
public sealed class DeadZoneTests
{
    [Test]
    public void AStickAtRestReadsAsExactlyZero()
    {
        Assert.AreEqual(Vector2.Zero, DeadZone.ApplyRadial(new Vector2(0.05f, -0.04f)));
    }

    [Test]
    public void ADiagonalNudgeIsIgnoredJustLikeAStraightOne()
    {
        var diagonal = new Vector2(0.1f, 0.1f);
        Assert.IsTrue(diagonal.Length < DeadZone.DefaultRadius, "this is inside the circle");
        Assert.AreEqual(Vector2.Zero, DeadZone.ApplyRadial(diagonal));
    }

    [Test]
    public void APushJustPastTheEdgeIsASmallPush_NotAJumpToFifteenPerCent()
    {
        Vector2 result = DeadZone.ApplyRadial(new Vector2(0.16f, 0.0f));
        Assert.IsTrue(result.Length > 0.0f);
        Assert.IsTrue(result.Length < 0.05f, "the remainder is rescaled from zero");
    }

    [Test]
    public void AFullPushIsStillAFullPush()
    {
        Assert.ApproximatelyEqual(1.0f, DeadZone.ApplyRadial(new Vector2(1.0f, 0.0f)).Length);
    }

    [Test]
    public void TheDirectionIsKeptWhateverTheMagnitude()
    {
        Vector2 result = DeadZone.ApplyRadial(new Vector2(0.6f, -0.8f));
        Assert.ApproximatelyEqual(-0.8f / 0.6f, result.Y / result.X, 1e-3f);
    }

    [Test]
    public void TheRadiusCanBeChanged()
    {
        Assert.AreEqual(Vector2.Zero, DeadZone.ApplyRadial(new Vector2(0.4f, 0.0f), 0.5f));
        Assert.IsTrue(DeadZone.ApplyRadial(new Vector2(0.4f, 0.0f), 0.1f).Length > 0.0f);
    }

    [Test]
    public void ATriggerHasAThresholdAndARampToo()
    {
        Assert.ApproximatelyEqual(0.0f, DeadZone.ApplyLinear(0.1f));
        Assert.ApproximatelyEqual(1.0f, DeadZone.ApplyLinear(1.0f));
        Assert.IsTrue(DeadZone.ApplyLinear(0.2f) > 0.0f);
        Assert.IsTrue(DeadZone.ApplyLinear(0.2f) < 0.2f);
    }

    [TestCase(0, -1.0f)]
    [TestCase(128, 0.0f)]
    [TestCase(255, 1.0f)]
    public void AnAxisByteMapsOntoMinusOneToOneWithTheCentreExact(int raw, float expected)
    {
        Assert.ApproximatelyEqual(expected, DeadZone.FromAxisByte((byte)raw));
    }

    [Test]
    public void TheTwoHalvesOfTheAxisAreScaledSeparately()
    {
        Assert.ApproximatelyEqual(-0.5f, DeadZone.FromAxisByte(64), 0.01f);
        Assert.ApproximatelyEqual(0.5f, DeadZone.FromAxisByte(192), 0.01f);
    }
}
