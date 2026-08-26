using GEngine.Core.Contracts;
using MarioClone.Tests.Doubles;
using GEngine.Testing;

namespace MarioClone.Tests;

/// <summary>
/// The four mercies that make a jump feel fair, each with a test that goes red if it is
/// removed: variable height, coyote time, the jump buffer, and the cut.
/// </summary>
public sealed class PlayerJumpTests
{
    private const string Flat = """
        tiles:
        ............
        ............
        ............
        ............
        ..M.........
        ############
        """;

    private const string Ledge = """
        tiles:
        ............
        ............
        ............
        ............
        ..M.........
        ####........
        """;

    [Test]
    public void APlayerStandingStillIsGrounded()
    {
        var test = new TestWorld(Flat);
        test.Step(10);
        Assert.IsTrue(test.Player.IsGrounded);
        Assert.ApproximatelyEqual(0.0f, test.Player.Body.Velocity.X);
    }

    [Test]
    public void AHeldJumpReachesAboutTwoAndAHalfTiles()
    {
        var test = new TestWorld(Flat);
        test.Step(5);
        float ground = test.Player.Bounds.Top;

        test.Hold(InputAction.Jump);
        float highest = test.HighestPointOf(test.Player, 60);

        float height = ground - highest;
        Assert.IsInRange(height, 24.0f, 34.0f, "between three and four tiles of clearance");
    }

    [Test]
    public void ATappedJumpIsAboutHalfTheHeightOfAHeldOne()
    {
        var held = new TestWorld(Flat);
        held.Step(5);
        float heldGround = held.Player.Bounds.Top;
        held.Hold(InputAction.Jump);
        float heldHeight = heldGround - held.HighestPointOf(held.Player, 60);

        var tapped = new TestWorld(Flat);
        tapped.Step(5);
        float tappedGround = tapped.Player.Bounds.Top;
        tapped.HoldFor(InputAction.Jump, 3);
        float tappedHeight = tappedGround - tapped.HighestPointOf(tapped.Player, 60);

        Assert.IsTrue(tappedHeight < heldHeight * 0.75f, "letting go early cuts the ascent short");
        Assert.IsTrue(tappedHeight > heldHeight * 0.2f, "but a tap is still a jump");
    }

    [Test]
    public void CoyoteTime_LetsAJumpLandJustAfterWalkingOffALedge()
    {
        var test = new TestWorld(Ledge);
        WalkOffTheEdge(test);
        Assert.IsFalse(test.Player.IsGrounded, "the player has walked off the end");
        Assert.IsTrue(test.Player.CoyoteSecondsLeft > 0.0f, "and is still owed a jump");

        float beforeJump = test.Player.Body.Velocity.Y;
        test.Hold(InputAction.MoveRight, InputAction.Jump);
        test.Step();
        Assert.IsTrue(test.Player.Body.Velocity.Y < beforeJump, "the jump was accepted in mid-air");
    }

    [Test]
    public void CoyoteTime_RunsOut()
    {
        var test = new TestWorld(Ledge);
        WalkOffTheEdge(test);
        test.Step(10);
        Assert.ApproximatelyEqual(0.0f, test.Player.CoyoteSecondsLeft, 1e-5f, "a long fall is a fall");

        float falling = test.Player.Body.Velocity.Y;
        test.Hold(InputAction.MoveRight, InputAction.Jump);
        test.Step();
        Assert.IsTrue(test.Player.Body.Velocity.Y > falling, "still falling, faster; the jump was refused");
    }

    // The buffer is a tenth of a second, so the press has to happen a tenth of a second before
    // landing - which is what this walks the player to, rather than guessing a step count.
    [Test]
    public void TheJumpBuffer_RemembersAPressMadeJustBeforeLanding()
    {
        const float FloorTop = 40.0f;
        var test = new TestWorld(Flat);
        test.Step(5);
        test.Hold(InputAction.Jump);
        test.Step(20);
        test.Release();

        while (!test.Player.IsGrounded && test.Player.Bounds.Bottom < FloorTop - 4.0f)
        {
            test.Step();
        }

        Assert.IsFalse(test.Player.IsGrounded, "not landed yet, but very nearly");

        test.Hold(InputAction.Jump);
        test.Step();
        test.Release();
        Assert.IsTrue(test.Player.JumpBufferSecondsLeft > 0.0f, "the press was remembered");

        test.Step(4);
        Assert.IsTrue(test.Player.Body.Velocity.Y < 0.0f, "the buffered jump fired on landing");
    }

    [Test]
    public void APlayerCannotJumpAgainWhileInTheAir()
    {
        var test = new TestWorld(Flat);
        test.Step(5);
        test.Hold(InputAction.Jump);
        test.Step(10);
        float rising = test.Player.Body.Velocity.Y;

        test.Release();
        test.Step();
        test.Hold(InputAction.Jump);
        test.Step();

        Assert.IsTrue(test.Player.Body.Velocity.Y > rising, "no second jump: the ascent kept slowing");
    }

    // Walks right until the ground runs out, and not one step further: coyote time is a tenth
    // of a second, so a test that walks for a fixed number of steps measures the wrong thing.
    private static void WalkOffTheEdge(TestWorld test)
    {
        test.Step(5);
        test.Hold(InputAction.MoveRight);
        for (int step = 0; step < 120 && test.Player.IsGrounded; step++)
        {
            test.Step();
        }
    }

    [Test]
    public void JumpingMakesTheJumpSound()
    {
        var test = new TestWorld(Flat);
        test.Step(5);
        test.Hold(InputAction.Jump);
        test.Step();
        Assert.AreEqual(1, test.Audio.CountOf(Audio.GameSound.Jump));
    }
}
