// Whether the camera is allowed to go back. An enum rather than a flag, because
// "camera.Scroll = CameraScroll.ForwardOnly" says what it does and "camera.NoBack = true"
// does not.

namespace GEngine.Rendering;

/// <summary>How a camera is allowed to move along the level.</summary>
public enum CameraScroll
{
    /// <summary>It follows the target in both directions.</summary>
    Free,

    /// <summary>
    /// It never moves back to the left. The original Super Mario Bros. does this, and it is
    /// why walking backwards squashes you against the edge of the screen instead of
    /// scrolling: the level behind you has already been given away to the next screen.
    /// </summary>
    ForwardOnly,
}
