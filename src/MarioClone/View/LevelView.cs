// Drawing the level: the tiles the camera can see, and then the actors on top of them.
//
// Only the visible tiles are drawn, and the range is arithmetic rather than a search - the
// camera's box divided by the tile size. A level ten times longer costs exactly the same.

using System;
using GEngine.Core;
using GEngine.Rendering;
using MarioClone.Actors;
using MarioClone.Game;
using MarioClone.Levels;

namespace MarioClone.View;

/// <summary>Draws a level and everything in it.</summary>
public sealed class LevelView
{
    private readonly SpriteAtlas _atlas;

    /// <summary>Creates a view.</summary>
    /// <param name="atlas">Where the sprites come from.</param>
    public LevelView(SpriteAtlas atlas)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        _atlas = atlas;
    }

    /// <summary>Draws the sky, the tiles and the actors.</summary>
    /// <param name="frame">Where to draw.</param>
    /// <param name="camera">What part of the world is on screen.</param>
    /// <param name="world">What to draw.</param>
    public void Draw(FrameBuffer frame, Camera2D camera, MarioWorld world)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(world);
        frame.Clear(Palette.Sky);
        DrawTiles(frame, camera, world.Level);
        foreach (Actor actor in world.Actors)
        {
            DrawActor(frame, camera, actor);
        }
    }

    private static void DrawTiles(FrameBuffer frame, Camera2D camera, Level level)
    {
        int firstColumn = Math.Max(0, MathG.FloorToInt(camera.View.Left / level.TileSize));
        int lastColumn = Math.Min(level.Columns - 1, MathG.CeilToInt(camera.View.Right / level.TileSize));
        int firstRow = Math.Max(0, MathG.FloorToInt(camera.View.Top / level.TileSize));
        int lastRow = Math.Min(level.Rows - 1, MathG.CeilToInt(camera.View.Bottom / level.TileSize));
        for (int row = firstRow; row <= lastRow; row++)
        {
            DrawTileRow(frame, camera, level, new TileRange(firstColumn, lastColumn, row));
        }
    }

    private static void DrawTileRow(FrameBuffer frame, Camera2D camera, Level level, TileRange range)
    {
        for (int column = range.FirstColumn; column <= range.LastColumn; column++)
        {
            TileKind kind = level.TileAt(column, range.Row);
            if (kind != TileKind.Empty)
            {
                DrawTile(frame, Screen(camera, level.TileBounds(column, range.Row)), kind, level.TileAt(column, range.Row - 1));
            }
        }
    }

    // The lighter strip along the top of a tile whose neighbour above is empty is what turns a
    // wall of identical squares into ground with a surface.
    private static void DrawTile(FrameBuffer frame, Aabb box, TileKind kind, TileKind above)
    {
        Color body = kind == TileKind.Pipe ? Palette.Green : Palette.Brown;
        Color top = kind == TileKind.Pipe ? Palette.LightGreen : Palette.LightBrown;
        frame.DrawRect(box, body);
        if (above == TileKind.Empty)
        {
            frame.DrawRect(new Aabb(box.Min, new Vector2(box.Max.X, box.Min.Y + 2.0f)), top);
        }
    }

    private void DrawActor(FrameBuffer frame, Camera2D camera, Actor actor)
    {
        if (!camera.IsVisible(actor.Bounds))
        {
            return;
        }

        if (actor is Goal)
        {
            DrawGoal(frame, camera, actor);
            return;
        }

        if (!_atlas.TryGet(actor.SpriteName, out Sprite? sprite) || sprite is null)
        {
            frame.DrawRect(Screen(camera, actor.Bounds), Palette.White);
            return;
        }

        Sprite drawn = actor.IsFacingLeft ? _atlas.GetMirrored(actor.SpriteName) : sprite;
        frame.DrawSprite(drawn, camera.WorldToScreen(actor.Bounds.Min));
    }

    private void DrawGoal(FrameBuffer frame, Camera2D camera, Actor goal)
    {
        Aabb box = Screen(camera, goal.Bounds);
        frame.DrawRect(new Aabb(new Vector2(box.Center.X - 1.0f, box.Top), new Vector2(box.Center.X + 1.0f, box.Bottom)), Palette.Grey);
        if (_atlas.TryGet("flag", out Sprite? flag) && flag is not null)
        {
            frame.DrawSprite(flag, new Vector2(box.Center.X, box.Top + 2.0f));
        }
    }

    private static Aabb Screen(Camera2D camera, Aabb world) =>
        new(camera.WorldToScreen(world.Min), camera.WorldToScreen(world.Max));

    private readonly struct TileRange
    {
        public TileRange(int firstColumn, int lastColumn, int row)
        {
            FirstColumn = firstColumn;
            LastColumn = lastColumn;
            Row = row;
        }

        public int FirstColumn { get; }

        public int LastColumn { get; }

        public int Row { get; }
    }
}
