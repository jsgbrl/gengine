// Turning a level file into a running world. This is the composition root of the game: the one
// place that knows a brick is a static body on the level layer, that a goomba walks left, and
// that the tilemap and the actors are two different things.

using System;
using GEngine.Core;
using GEngine.Input.Actions;
using GEngine.Physics;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tiles;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Levels;

namespace MarioClone.Game;

/// <summary>Builds a playable world out of a level.</summary>
public static class MarioFactory
{
    /// <summary>How wide a grid cell is, in pixels. Everything in the game is a multiple of it.</summary>
    public const float TileSizePixels = 8.0f;

    /// <summary>Builds a world from a level.</summary>
    /// <param name="level">The level to build.</param>
    /// <param name="input">Where the player's decisions come from.</param>
    /// <param name="session">The score, coins and lives to carry into it.</param>
    /// <param name="audio">Where sound goes.</param>
    /// <returns>The world, with everything in it and the player at the start.</returns>
    public static MarioWorld Build(Level level, InputState input, GameSession session, IAudioBackend audio)
    {
        ArgumentNullException.ThrowIfNull(level);
        var settings = new PhysicsSettings
        {
            Gravity = new Vector2(0.0f, PlayerTuning.Default.GravityPixelsPerSecondSquared),
        };

        var physics = new PhysicsWorld(settings, new SpatialHashGrid(level.TileSize * 4.0f))
        {
            Tiles = BuildTiles(level),
        };

        var world = new MarioWorld(level, physics, session, audio);
        foreach (LevelSpawn spawn in level.Spawns)
        {
            Place(world, spawn, input);
        }

        return world;
    }

    /// <summary>Turns the level's grid into the collision source the physics world reads.</summary>
    /// <param name="level">The level whose grid to convert.</param>
    /// <returns>The tilemap.</returns>
    public static TileCollisionSource BuildTiles(Level level)
    {
        ArgumentNullException.ThrowIfNull(level);
        var tiles = new TileCollisionSource(level.Columns, level.Rows, level.TileSize);
        for (int row = 0; row < level.Rows; row++)
        {
            for (int column = 0; column < level.Columns; column++)
            {
                TileKind kind = level.TileAt(column, row);
                tiles.Set(column, row, kind == TileKind.Empty ? TileCollision.None : TileCollision.Solid);
            }
        }

        return tiles;
    }

    private static void Place(MarioWorld world, LevelSpawn spawn, InputState input)
    {
        Vector2 center = world.Level.TileCenter(spawn.Column, spawn.Row);
        switch (spawn.Kind)
        {
            case SpawnKind.Player:
                world.Add(new Player(Body(center, Player.WidthPixels, Player.SmallHeightPixels), input, PlayerTuning.Default), "player");
                break;
            case SpawnKind.Goomba:
                world.Add(new Goomba(Body(center, Goomba.SizePixels, Goomba.SizePixels)), "goomba");
                break;
            case SpawnKind.Coin:
                world.Add(new Coin(Body(center, Coin.SizePixels, Coin.SizePixels)), "coin");
                break;
            default:
                PlaceBlock(world, spawn, center);
                break;
        }
    }

    // A brick, a question block and the flag are the three things that fill the cell they came
    // from, which is why they are the three that need to know how big a cell is.
    private static void PlaceBlock(MarioWorld world, LevelSpawn spawn, Vector2 center)
    {
        float size = world.Level.TileSize;
        switch (spawn.Kind)
        {
            case SpawnKind.Brick:
                world.Add(new Brick(Body(center, size, size)), "brick");
                break;
            case SpawnKind.QuestionBlock:
                world.Add(new QuestionBlock(Body(center, size, size), PrizeFor(spawn)), "block");
                break;
            default:
                PlaceGoal(world, spawn);
                break;
        }
    }

    // The flag stands on the ground and reaches up, so its body is anchored at the bottom of
    // its tile rather than centred on it.
    private static void PlaceGoal(MarioWorld world, LevelSpawn spawn)
    {
        float bottom = world.Level.TileBounds(spawn.Column, spawn.Row).Bottom;
        var center = new Vector2(
            world.Level.TileCenter(spawn.Column, spawn.Row).X,
            bottom - (Goal.HeightPixels / 2.0f));

        world.Add(new Goal(Body(center, Goal.WidthPixels, Goal.HeightPixels)), "goal");
    }

    // Every third block holds a mushroom, which is close enough to the original's rhythm and
    // is a rule rather than a list.
    private static BlockPrize PrizeFor(LevelSpawn spawn) =>
        spawn.Column % 3 == 1 ? BlockPrize.Mushroom : BlockPrize.Coin;

    private static RigidBody2D Body(Vector2 center, float width, float height) =>
        new(BodyType.Dynamic, center, new Vector2(width, height));
}
