// Identities are handed out by a world, so a broad-phase test that needs bodies with
// distinct ids borrows an empty world to number them.

using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.BroadPhase;

namespace GEngine.Physics.Tests.Doubles;

internal sealed class BodySet
{
    private readonly PhysicsWorld _world = new(PhysicsSettings.Default, new BruteForceBroadPhase());

    public IReadOnlyList<RigidBody2D> All => _world.Bodies;

    public RigidBody2D Add(BodyType type, Vector2 position, Vector2 size) =>
        _world.Add(new RigidBody2D(type, position, size));

    public RigidBody2D AddBoxAt(float x, float y) =>
        Add(BodyType.Dynamic, new Vector2(x, y), new Vector2(10.0f, 10.0f));
}
