// How the player moves, and the four small mercies that make a platformer feel fair.
//
//   coyote time   you may still jump for a tenth of a second after walking off a ledge
//   jump buffer   a jump pressed just before landing fires the moment you land
//   jump cut      letting the button go early shortens the ascent
//   skid          turning around decelerates faster than accelerating does
//
// None of them is realistic. All of them are what the player thinks already happened, and a
// game without them feels like it is ignoring you - which the player blames on themselves,
// and then stops playing.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using MarioClone.Audio;

namespace MarioClone.Actors;

/// <content>Movement, jumping, and falling out of the world.</content>
public sealed partial class Player
{
    /// <inheritdoc/>
    protected override void OnFixedUpdate(float fixedDeltaSeconds)
    {
        if (IsDead || HasReachedGoal)
        {
            Coast();
            return;
        }

        RunTimers(fixedDeltaSeconds);
        Walk(fixedDeltaSeconds);
        Jump();
        CheckPit();
    }

    private void RunTimers(float fixedDeltaSeconds)
    {
        _invulnerableSeconds = MathF.Max(0.0f, _invulnerableSeconds - fixedDeltaSeconds);
        _coyoteSeconds = IsGrounded
            ? Tuning.CoyoteTimeSeconds
            : MathF.Max(0.0f, _coyoteSeconds - fixedDeltaSeconds);

        _jumpBufferSeconds = _input.WasPressedThisFrame(InputAction.Jump)
            ? Tuning.JumpBufferSeconds
            : MathF.Max(0.0f, _jumpBufferSeconds - fixedDeltaSeconds);
    }

    // The stick gives a number from zero to one and a key gives exactly one, and both go
    // through this same line: the target speed is the direction times the top speed. That is
    // the whole reason analogue and digital movement feel like the same game.
    private void Walk(float fixedDeltaSeconds)
    {
        float direction = _input.AxisValue(InputAction.MoveRight) - _input.AxisValue(InputAction.MoveLeft);
        float top = _input.IsDown(InputAction.Run) ? Tuning.RunSpeedPixelsPerSecond : Tuning.WalkSpeedPixelsPerSecond;
        float target = direction * top;
        float rate = RateTowards(target, fixedDeltaSeconds);
        Body.Velocity = Body.Velocity.WithX(MathG.MoveTowards(Body.Velocity.X, target, rate));
        if (MathF.Abs(direction) > 0.01f)
        {
            IsFacingLeft = direction < 0.0f;
        }
    }

    private float RateTowards(float target, float fixedDeltaSeconds)
    {
        float current = Body.Velocity.X;
        bool isTurning = target != 0.0f && current != 0.0f && MathF.Sign(target) != MathF.Sign(current);
        float acceleration = target == 0.0f
            ? Tuning.FrictionPixelsPerSecondSquared
            : isTurning ? Tuning.SkidPixelsPerSecondSquared : Tuning.AccelerationPixelsPerSecondSquared;

        float airFactor = IsGrounded ? 1.0f : Tuning.AirControl;
        return acceleration * airFactor * fixedDeltaSeconds;
    }

    private void Jump()
    {
        if (_jumpBufferSeconds > 0.0f && _coyoteSeconds > 0.0f)
        {
            Body.Velocity = Body.Velocity.WithY(Tuning.JumpVelocityPixelsPerSecond);
            _jumpBufferSeconds = 0.0f;
            _coyoteSeconds = 0.0f;
            _isRising = true;
            World.Audio.Play(GameSound.Jump);
        }

        // The cut is what makes the jump height variable: the ascent keeps its full speed only
        // while the button is held, and a tap gives about half the height of a hold.
        if (_isRising && !_input.IsDown(InputAction.Jump) && Body.Velocity.Y < 0.0f)
        {
            Body.Velocity = Body.Velocity.WithY(Body.Velocity.Y * Tuning.JumpCutFactor);
            _isRising = false;
        }

        if (Body.Velocity.Y >= 0.0f)
        {
            _isRising = false;
        }
    }

    // Nothing here falls: gravity belongs to the physics world, terminal velocity is the
    // body's MaxVelocity, and both are set once from PlayerTuning when the level is built.
    private void Coast()
    {
        Body.Velocity = Body.Velocity.WithX(0.0f);
    }

    private void CheckPit()
    {
        if (Body.Bounds.Top > World.Level.Bounds.Bottom + Tuning.PitDepthPixels)
        {
            Kill();
        }
    }
}
