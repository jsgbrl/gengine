using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>Walking, running, turning around, and the promise that a stick feels like a key.</summary>
public sealed class PlayerMovementTests
{
    private const string Flat = """
        tiles:
        ............
        ............
        ............
        ..M.........
        ############
        """;

    [Test]
    public void HoldingRightWalksRightAndFacesRight()
    {
        var test = new TestWorld(Flat);
        test.Step(5);
        float start = test.Player.Position.X;
        test.Hold(InputAction.MoveRight);
        test.Step(30);
        Assert.IsTrue(test.Player.Position.X > start);
        Assert.IsFalse(test.Player.IsFacingLeft);
    }

    [Test]
    public void HoldingLeftWalksLeftAndFacesLeft()
    {
        var test = new TestWorld(Flat);
        test.Step(5);
        float start = test.Player.Position.X;
        test.Hold(InputAction.MoveLeft);
        test.Step(30);
        Assert.IsTrue(test.Player.Position.X < start);
        Assert.IsTrue(test.Player.IsFacingLeft);
    }

    [Test]
    public void TopSpeedIsTheWalkSpeed()
    {
        var test = new TestWorld(Flat);
        test.Hold(InputAction.MoveRight);
        test.Step(60);
        Assert.ApproximatelyEqual(PlayerTuning.Default.WalkSpeedPixelsPerSecond, test.Player.Body.Velocity.X, 0.5f);
    }

    [Test]
    public void HoldingRunReachesTheRunSpeed()
    {
        var test = new TestWorld(Flat);
        test.Hold(InputAction.MoveRight, InputAction.Run);
        test.Step(60);
        Assert.ApproximatelyEqual(PlayerTuning.Default.RunSpeedPixelsPerSecond, test.Player.Body.Velocity.X, 0.5f);
    }

    [Test]
    public void LettingGoStopsThePlayerRatherThanFreezingThem()
    {
        var test = new TestWorld(Flat);
        test.Hold(InputAction.MoveRight);
        test.Step(30);
        Assert.IsTrue(test.Player.Body.Velocity.X > 10.0f);

        test.Release();
        test.Step(2);
        Assert.IsTrue(test.Player.Body.Velocity.X > 0.0f, "it slows down rather than stopping dead");
        test.Step(20);
        Assert.ApproximatelyEqual(0.0f, test.Player.Body.Velocity.X, 0.5f);
    }

    [Test]
    public void TurningAroundIsFasterThanAcceleratingFromRest()
    {
        var turning = new TestWorld(Flat);
        turning.Hold(InputAction.MoveRight);
        turning.Step(40);
        turning.Hold(InputAction.MoveLeft);
        turning.Step(6);
        float afterSkid = turning.Player.Body.Velocity.X;

        var starting = new TestWorld(Flat);
        starting.Step(40);
        starting.Hold(InputAction.MoveLeft);
        starting.Step(6);
        float afterStart = starting.Player.Body.Velocity.X;

        float skidChange = PlayerTuning.Default.WalkSpeedPixelsPerSecond - afterSkid;
        float startChange = -afterStart;
        Assert.IsTrue(skidChange > startChange, "a skid sheds speed faster than a standing start builds it");
    }

    // The stick reports a fraction and a key reports one; both go through the same line, which
    // is what makes analogue and digital movement the same game rather than two.
    [Test]
    public void AStickHalfWayGivesAboutHalfTheSpeedOfAKey()
    {
        var digital = new TestWorld(Flat);
        digital.Hold(InputAction.MoveRight);
        digital.Step(60);

        var analogue = new TestWorld(Flat);
        for (int step = 0; step < 60; step++)
        {
            analogue.Input.Begin();
            analogue.Input.Report(InputAction.MoveRight, 0.5f);
            analogue.Input.End(TestWorld.FixedDelta);
            analogue.World.Step(TestWorld.FixedDelta);
        }

        Assert.ApproximatelyEqual(digital.Player.Body.Velocity.X * 0.5f, analogue.Player.Body.Velocity.X, 1.0f);
    }

    [Test]
    public void ThePlayerHasLessControlInTheAirThanOnTheGround()
    {
        var ground = new TestWorld(Flat);
        ground.Step(5);
        ground.Hold(InputAction.MoveRight);
        ground.Step(4);

        var air = new TestWorld(Flat);
        air.Step(5);
        air.Hold(InputAction.Jump);
        air.Step(6);
        air.Hold(InputAction.MoveRight, InputAction.Jump);
        air.Step(4);

        Assert.IsTrue(air.Player.Body.Velocity.X < ground.Player.Body.Velocity.X);
        Assert.IsTrue(air.Player.Body.Velocity.X > 0.0f, "but there is still control");
    }

    [Test]
    public void FallingIsCappedAtTerminalVelocity()
    {
        var test = new TestWorld("""
            tiles:
            ..M.........
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            ............
            """);

        test.Step(35);
        Assert.ApproximatelyEqual(
            PlayerTuning.Default.MaximumFallSpeedPixelsPerSecond,
            test.Player.Body.Velocity.Y,
            1.0f);
    }
}
