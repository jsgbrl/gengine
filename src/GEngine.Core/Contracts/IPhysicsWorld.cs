// What the composition root needs from a physics world, and no more.
//
// The queries a game makes - raycasts, overlap tests, adding bodies - live on the concrete
// PhysicsWorld, because describing them here would mean describing a rigid body here, and Core
// would have absorbed the module it is supposed to be independent of.

namespace GEngine.Core.Contracts;

/// <summary>Something the fixed step can advance.</summary>
public interface IPhysicsWorld
{
    /// <summary>Acceleration applied to every dynamic body, in pixels per second squared.</summary>
    Vector2 Gravity { get; set; }

    /// <summary>Advances the simulation by exactly one fixed step.</summary>
    /// <param name="fixedDeltaSeconds">The fixed step, in seconds. Always the same value.</param>
    void Step(float fixedDeltaSeconds);
}
