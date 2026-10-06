using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Physics;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Levels;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The world holds three views of the same set of things - a list of actors, a scene of
/// entities, and a list of bodies in the physics world - and its whole job is keeping them
/// saying the same thing. Adding and retiring an actor is where that either works or does not.
/// </summary>
public sealed class MarioWorldTests
{
    // The actor list and the physics world take it now; the scene takes it on the next step,
    // because a scene that added an entity while it was iterating its own list would be
    // rearranging the floor under the step that asked for the entity.
    [Test]
    public void AddingAnActorAddsItToTheListAndThePhysicsAtOnce()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        world.Step();
        int actors = world.World.Actors.Count;
        int bodies = world.World.Physics.Bodies.Count;
        int entities = world.World.Scene.Entities.Count;

        world.World.Add(new Coin(Body(new Vector2(20.0f, 4.0f))), "coin");

        Assert.AreEqual(actors + 1, world.World.Actors.Count);
        Assert.AreEqual(bodies + 1, world.World.Physics.Bodies.Count);
        Assert.AreEqual(entities, world.World.Scene.Entities.Count, "the scene has not been stepped yet");
        Assert.AreEqual(1, world.World.Scene.PendingCount);

        world.Step();
        Assert.AreEqual(entities + 1, world.World.Scene.Entities.Count, "and now it is in the scene");
    }

    [Test]
    public void AnActorAndItsBodyKnowAboutEachOther()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        var coin = new Coin(Body(new Vector2(20.0f, 4.0f)));
        world.World.Add(coin, "coin");

        Assert.AreSame(coin, coin.Body.Owner, "a contact names a body, and the body names the actor");
        Assert.AreSame(world.World, coin.World);
    }

    [Test]
    public void ThePlayerIsFoundAsItIsAdded()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        Assert.IsNotNull(world.World.Player);
        Assert.AreSame(world.World.Player, world.Find<Player>());
    }

    [Test]
    public void AWorldWithNoStartHasNoPlayer()
    {
        Level level = LevelLoader.Parse("tiles:\n...\n###\n", MarioFactory.TileSizePixels);
        MarioWorld world = MarioFactory.Build(level, new InputState(), new GameSession(), NullAudioBackend.Instance);
        Assert.IsNull(world.Player, "no start marker, no player, and no exception either");
    }

    // Removal happens after the solver, never inside it: taking a body out mid-step is the
    // classic way to lose the contact that was about to fire.
    [Test]
    public void ARetiredActorLeavesEverywhereOnTheSameStep()
    {
        var world = new TestWorld("tiles:\n.M..o.\n######\n");
        int actors = world.World.Actors.Count;
        int bodies = world.World.Physics.Bodies.Count;
        world.Step();
        int entities = world.World.Scene.Entities.Count;

        world.Hold(InputAction.MoveRight);
        world.Step(60);

        Assert.IsNull(world.Find<Coin>(), "the coin was collected on the way past");
        Assert.AreEqual(actors - 1, world.World.Actors.Count);
        Assert.AreEqual(bodies - 1, world.World.Physics.Bodies.Count, "and its body went with it");
        Assert.AreEqual(entities - 1, world.World.Scene.Entities.Count, "and its entity too");
    }

    [Test]
    public void AMushroomComesOutOnTopOfTheBlockThatGaveIt()
    {
        var world = new TestWorld("tiles:\n.M.?.\n#####\n");
        QuestionBlock block = world.Find<QuestionBlock>()!;
        Mushroom mushroom = world.World.SpawnMushroomAbove(block);

        Assert.ApproximatelyEqual(block.Position.X, mushroom.Position.X, 0.001f, "directly above it");
        Assert.IsTrue(mushroom.Bounds.Bottom <= block.Bounds.Top, "and not inside it");
        Assert.AreSame(mushroom, world.Find<Mushroom>());
    }

    [Test]
    public void AWorldRefusesNothingWhereAnActorShouldBe()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        Assert.Throws<ArgumentNullException>(() => world.World.Add(null!, "nothing"));
        Assert.Throws<ArgumentNullException>(() => world.World.SpawnMushroomAbove(null!));
    }

    [Test]
    public void SteppingTheWorldStepsThePhysics()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        long steps = world.World.Physics.StepCount;
        world.Step(3);
        Assert.AreEqual(steps + 3, world.World.Physics.StepCount);
    }

    private static RigidBody2D Body(Vector2 position) =>
        new(BodyType.Static, position, new Vector2(6.0f, 6.0f)) { IsTrigger = true };
}
