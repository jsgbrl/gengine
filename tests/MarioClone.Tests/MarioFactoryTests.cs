using System;
using GEngine.Input.Actions;
using GEngine.Physics;
using GEngine.Physics.Tiles;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Levels;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The composition root: the one place that knows a brick is a static body on the level layer
/// and a goomba walks left. Everything it decides is a decision that would otherwise be spread
/// across nine actor classes, so it is worth checking that it decides all of it in one place.
/// </summary>
public sealed class MarioFactoryTests
{
    [Test]
    public void EverySpawnInTheLevelBecomesAnActor()
    {
        var world = new TestWorld("tiles:\n.M.o.G.B.?.F.\n#############\n");
        Assert.AreEqual(1, world.CountOf<Player>());
        Assert.AreEqual(1, world.CountOf<Coin>());
        Assert.AreEqual(1, world.CountOf<Goomba>());
        Assert.AreEqual(1, world.CountOf<Brick>());
        Assert.AreEqual(1, world.CountOf<QuestionBlock>());
        Assert.AreEqual(1, world.CountOf<Goal>());
    }

    [Test]
    public void ThePlayerStartsWhereTheLevelSaidToStart()
    {
        var world = new TestWorld("tiles:\n..M..\n#####\n");
        Assert.ApproximatelyEqual(world.Level.PlayerStart.X, world.Player.Position.X, 0.001f);
        Assert.ApproximatelyEqual(world.Level.PlayerStart.Y, world.Player.Position.Y, 0.001f);
    }

    [Test]
    public void GravityComesFromThePlayerTuningAndBelongsToTheWorld()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        Assert.ApproximatelyEqual(
            PlayerTuning.Default.GravityPixelsPerSecondSquared,
            world.World.Physics.Gravity.Y,
            0.001f,
            "one gravity, in the physics world, not one per actor");
        Assert.ApproximatelyEqual(0.0f, world.World.Physics.Gravity.X, 0.001f);
    }

    // The tilemap is not a pile of bodies: a hundred and forty columns of floor would be a
    // hundred and forty static boxes in the broad phase, and it is one grid lookup instead.
    [Test]
    public void TheGroundIsTilesRatherThanBodies()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        Assert.IsNotNull(world.World.Physics.Tiles);
        Assert.AreEqual(1, world.World.Physics.Bodies.Count, "the player, and nothing else");
    }

    [Test]
    public void SolidTilesBecomeSolidCellsAndSkyDoesNot()
    {
        Level level = LevelLoader.Parse("tiles:\n.P.\n###\n", MarioFactory.TileSizePixels);
        TileCollisionSource tiles = MarioFactory.BuildTiles(level);
        Assert.AreEqual(TileCollision.Solid, tiles.At(1, 0), "a pipe is solid");
        Assert.AreEqual(TileCollision.Solid, tiles.At(0, 1), "so is the ground");
        Assert.AreEqual(TileCollision.None, tiles.At(0, 0), "sky is not");
    }

    [Test]
    public void TheTilemapIsTheSameShapeAsTheLevel()
    {
        Level level = LevelLoader.Parse("tiles:\n....\n####\n", MarioFactory.TileSizePixels);
        TileCollisionSource tiles = MarioFactory.BuildTiles(level);
        Assert.AreEqual(level.Columns, tiles.Columns);
        Assert.AreEqual(level.Rows, tiles.Rows);
        Assert.ApproximatelyEqual(level.TileSize, tiles.TileSize, 0.001f);
    }

    [Test]
    public void ActorsAreOnTheLayerThatMatchesWhatTheyAre()
    {
        var world = new TestWorld("tiles:\n.M.o.G.\n#######\n");
        Assert.AreEqual(CollisionLayers.Player, world.Player.Body.Layer);
        Assert.AreEqual(CollisionLayers.Pickup, world.Find<Coin>()!.Body.Layer);
        Assert.AreEqual(CollisionLayers.Enemy, world.Find<Goomba>()!.Body.Layer);
    }

    [Test]
    public void ThingsYouWalkThroughAreTriggersAndThingsYouStandOnAreNot()
    {
        var world = new TestWorld("tiles:\n.M.o.B.\n#######\n");
        Assert.IsTrue(world.Find<Coin>()!.Body.IsTrigger, "a coin is collected, not bumped into");
        Assert.IsFalse(world.Find<Brick>()!.Body.IsTrigger, "a brick is something you land on");
        Assert.AreEqual(BodyType.Static, world.Find<Brick>()!.Body.Type);
    }

    [Test]
    public void AWorldWithNoLevelIsRefused()
    {
        Assert.Throws<ArgumentNullException>(
            () => MarioFactory.Build(null!, new InputState(), new GameSession(), NullAudioBackend.Instance));
        Assert.Throws<ArgumentNullException>(() => MarioFactory.BuildTiles(null!));
    }
}
