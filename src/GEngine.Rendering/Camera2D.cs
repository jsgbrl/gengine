// Where the view is. A camera that simply centres on the player twitches with every step he
// takes, so the target is allowed to wander inside a dead zone in the middle of the screen
// before the view moves at all - and then the view moves by exactly the excess, which means
// it never overshoots and never has to spring back.

using System;
using GEngine.Core;

namespace GEngine.Rendering;

/// <summary>A rectangular view onto the world, with a dead zone and a clamp.</summary>
public sealed class Camera2D
{
    /// <summary>Creates a camera.</summary>
    /// <param name="viewSize">How much of the world fits on screen, in pixels.</param>
    public Camera2D(Vector2 viewSize)
    {
        ViewSize = viewSize;
        DeadZone = viewSize * 0.15f;
    }

    /// <summary>Top-left corner of the view, in world pixels.</summary>
    public Vector2 Position { get; private set; }

    /// <summary>How much of the world fits on screen, in pixels.</summary>
    public Vector2 ViewSize { get; }

    /// <summary>The part of the world currently on screen.</summary>
    public Aabb View => new(Position, Position + ViewSize);

    /// <summary>Half-extents of the box in the middle of the screen the target may wander in.</summary>
    public Vector2 DeadZone { get; set; }

    /// <summary>The whole level, which the view is kept inside. A zero-sized box means no clamp.</summary>
    public Aabb LevelBounds { get; set; }

    /// <summary>Whether the camera may move back the way it came.</summary>
    public CameraScroll Scroll { get; set; } = CameraScroll.Free;

    /// <summary>Moves the view only as far as the target leaving the dead zone requires.</summary>
    /// <param name="target">Where to keep in view, in world pixels.</param>
    public void Follow(Vector2 target)
    {
        Vector2 offset = target - (Position + (ViewSize * 0.5f));
        MoveTo(Position + new Vector2(Excess(offset.X, DeadZone.X), Excess(offset.Y, DeadZone.Y)));
    }

    /// <summary>Centres the view on a point at once, ignoring the dead zone.</summary>
    /// <param name="target">Where to centre, in world pixels.</param>
    public void SnapTo(Vector2 target) => MoveTo(target - (ViewSize * 0.5f));

    /// <summary>Turns a world position into a position in the frame buffer.</summary>
    /// <param name="world">The world position, in pixels.</param>
    /// <returns>The screen position, in pixels.</returns>
    public Vector2 WorldToScreen(Vector2 world) => world - Position;

    /// <summary>Turns a position in the frame buffer into a world position.</summary>
    /// <param name="screen">The screen position, in pixels.</param>
    /// <returns>The world position, in pixels.</returns>
    public Vector2 ScreenToWorld(Vector2 screen) => screen + Position;

    /// <summary>True when a box is at least partly on screen.</summary>
    /// <param name="box">The box, in world pixels.</param>
    /// <returns>True when it is worth drawing.</returns>
    public bool IsVisible(Aabb box) => View.Intersects(box);

    private static float Excess(float offset, float deadZone)
    {
        if (MathF.Abs(offset) <= deadZone)
        {
            return 0.0f;
        }

        return offset - (MathF.Sign(offset) * deadZone);
    }

    private void MoveTo(Vector2 wanted)
    {
        Vector2 clamped = ClampToLevel(wanted);
        if (Scroll == CameraScroll.ForwardOnly)
        {
            clamped = clamped.WithX(MathF.Max(clamped.X, Position.X));
        }

        Position = clamped;
    }

    private Vector2 ClampToLevel(Vector2 wanted)
    {
        Vector2 size = LevelBounds.Size;
        if (size.X <= 0.0f || size.Y <= 0.0f)
        {
            return wanted;
        }

        float lastX = MathF.Max(LevelBounds.Min.X, LevelBounds.Max.X - ViewSize.X);
        float lastY = MathF.Max(LevelBounds.Min.Y, LevelBounds.Max.Y - ViewSize.Y);
        return new Vector2(
            MathG.Clamp(wanted.X, LevelBounds.Min.X, lastX),
            MathG.Clamp(wanted.Y, LevelBounds.Min.Y, lastY));
    }
}
