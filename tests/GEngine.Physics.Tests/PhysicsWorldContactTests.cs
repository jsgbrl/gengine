using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>
/// Covers the contact bookkeeping: enter once, stay while it lasts, exit once. Getting this
/// wrong is how a coin is collected twice or a goomba is stomped for ever.
/// </summary>
public sealed class PhysicsWorldContactTests
{
    private const float FixedDelta = 1.0f / 60.0f;

    private PhysicsWorld _world = new(PhysicsSettings.Default, new SpatialHashGrid());
    private RecordingListener _listener = new();

    [Setup]
    public void Setup()
    {
        _world = new PhysicsWorld(PhysicsSettings.Default, new SpatialHashGrid()) { Gravity = Vector2.Zero };
        _listener = new RecordingListener();
    }

    // With gravity on, a body resting on a floor pushes into it every step, so the contact
    // is renewed every step. With gravity off and the velocity spent there is no movement
    // and no overlap, and the engine reports nothing further: a contact here is a touch
    // this step produced, not a permanent relationship. docs/physics.md says so out loud.
    [Test]
    public void LandingOnAStaticBody_FiresEnterOnceAndThenStay()
    {
        _world.Gravity = PhysicsSettings.Default.Gravity;
        RigidBody2D floor = AddStatic(new Vector2(0.0f, 100.0f), new Vector2(200.0f, 20.0f));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 80.0f));
        body.Listener = _listener;
        body.Velocity = new Vector2(0.0f, 300.0f);

        for (int step = 0; step < 10; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.AreEqual(1, _listener.CollisionEnterCount);
        Assert.IsTrue(_listener.CollisionStayCount > 1);
        Assert.AreEqual(0, _listener.CollisionExitCount);
        Assert.AreSame(floor, _listener.LastContact.Other);
    }

    [Test]
    public void TheNormalOfALandingPointsUp_WhichIsHowAStompIsRecognised()
    {
        AddStatic(new Vector2(0.0f, 100.0f), new Vector2(200.0f, 20.0f));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 80.0f));
        body.Listener = _listener;
        body.Velocity = new Vector2(0.0f, 300.0f);

        for (int step = 0; step < 10; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.AreEqual(new Vector2(0.0f, -1.0f), _listener.LastContact.Normal);
    }

    [Test]
    public void BothSidesOfAContactAreTold_AndEachSeesTheNormalFromItsOwnSide()
    {
        var otherListener = new RecordingListener();
        RigidBody2D left = AddDynamic(new Vector2(0.0f, 0.0f));
        RigidBody2D right = AddDynamic(new Vector2(6.0f, 0.0f));
        left.Listener = _listener;
        right.Listener = otherListener;

        _world.Step(FixedDelta);

        Assert.AreEqual(1, _listener.CollisionEnterCount);
        Assert.AreEqual(1, otherListener.CollisionEnterCount);
        Assert.AreEqual(-_listener.LastContact.Normal, otherListener.LastContact.Normal);
    }

    [Test]
    public void MovingApart_FiresExitExactlyOnce()
    {
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 0.0f));
        RigidBody2D wall = AddStatic(new Vector2(6.0f, 0.0f), new Vector2(10.0f, 10.0f));
        body.Listener = _listener;

        _world.Step(FixedDelta);
        Assert.AreEqual(1, _listener.CollisionEnterCount);

        _world.Remove(wall);
        _world.Step(FixedDelta);
        _world.Step(FixedDelta);
        Assert.AreEqual(0, _listener.CollisionExitCount, "a body that left the world is not a body that let go");
    }

    [Test]
    public void ABodyThatSlidesAwayFromAWall_FiresExitOnce()
    {
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 0.0f));
        AddStatic(new Vector2(6.0f, 0.0f), new Vector2(10.0f, 10.0f));
        body.Listener = _listener;
        body.Velocity = new Vector2(200.0f, 0.0f);

        _world.Step(FixedDelta);
        Assert.AreEqual(1, _listener.CollisionEnterCount);

        body.Velocity = new Vector2(0.0f, -2000.0f);
        _world.Step(FixedDelta);
        _world.Step(FixedDelta);
        Assert.AreEqual(1, _listener.CollisionExitCount);
    }

    [Test]
    public void ATriggerReportsOverlapsAndStopsNothing()
    {
        RigidBody2D coin = AddStatic(new Vector2(60.0f, 0.0f), new Vector2(10.0f, 10.0f));
        coin.IsTrigger = true;
        coin.Listener = _listener;
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 0.0f));
        body.Velocity = new Vector2(600.0f, 0.0f);

        for (int step = 0; step < 20; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.AreEqual(1, _listener.TriggerEnterCount);
        Assert.AreEqual(1, _listener.TriggerExitCount);
        Assert.AreEqual(0, _listener.CollisionEnterCount, "a trigger never reports a collision");
        Assert.IsTrue(body.Position.X > 100.0f, "the body passed straight through");
    }

    [Test]
    public void LayersThatIgnoreEachOther_ProduceNoContactAtAll()
    {
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 0.0f));
        RigidBody2D ghost = AddStatic(new Vector2(6.0f, 0.0f), new Vector2(10.0f, 10.0f));
        body.Listener = _listener;
        body.Layer = 0b01;
        body.Mask = 0b01;
        ghost.Layer = 0b10;
        ghost.Mask = 0b10;

        _world.Step(FixedDelta);

        Assert.AreEqual(0, _listener.CollisionEnterCount);
    }

    [Test]
    public void ContactCount_ReportsHowManyPairsAreTouching()
    {
        AddDynamic(new Vector2(0.0f, 0.0f));
        AddDynamic(new Vector2(6.0f, 0.0f));
        _world.Step(FixedDelta);
        Assert.AreEqual(1, _world.ContactCount);
    }

    private RigidBody2D AddDynamic(Vector2 position) =>
        _world.Add(new RigidBody2D(BodyType.Dynamic, position, new Vector2(10.0f, 10.0f)));

    private RigidBody2D AddStatic(Vector2 position, Vector2 size) =>
        _world.Add(new RigidBody2D(BodyType.Static, position, size));
}
