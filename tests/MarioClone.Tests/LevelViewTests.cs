using System;
using GEngine.Core;
using GEngine.Rendering;
using GEngine.Rendering.Assets;
using GEngine.Testing;
using MarioClone.Game;
using MarioClone.Tests.Doubles;
using MarioClone.View;

namespace MarioClone.Tests;

/// <summary>
/// The view draws the sky, the tiles the camera can see, and the actors. Its only real job is
/// culling: a level is a hundred and forty columns wide and a frame shows twenty, so drawing
/// all of them would be seven times the work for the same picture.
/// </summary>
public sealed class LevelViewTests
{
    [Test]
    public void AnEmptyStretchOfLevelIsAllSky()
    {
        var frame = new FrameBuffer(64, 48);
        Draw(frame, World("tiles:\n.M...\n#####\n"), Camera(frame, new Vector2(200.0f, 200.0f)));
        Assert.AreEqual(Palette.Sky, frame.GetPixel(32, 24), "past the end of the level there is only sky");
    }

    [Test]
    public void TheGroundIsDrawnWhereTheGroundIs()
    {
        var frame = new FrameBuffer(64, 48);
        TestWorld world = World("tiles:\n.M...\n#####\n");
        Draw(frame, world, Camera(frame, new Vector2(20.0f, 8.0f)));
        Assert.IsTrue(HasSomethingOtherThanSky(frame), "the floor was drawn");
    }

    // The picture must not depend on how far along the level the camera happens to be: two
    // identical stretches of floor, seen the same way, are the same picture. The player stands
    // at the far left, out of both views, so what is compared is the tilemap and nothing else.
    [Test]
    public void TheSameStretchOfLevelLooksTheSameWhereverItIs()
    {
        TestWorld world = World("tiles:\n.M..................\n####################\n");
        var near = new FrameBuffer(48, 32);
        var far = new FrameBuffer(48, 32);
        Draw(near, world, Camera(near, new Vector2(80.0f, 8.0f)));
        Draw(far, world, Camera(far, new Vector2(112.0f, 8.0f)));

        Assert.MatchesSnapshot(AsciiSnapshot.Capture(far), AsciiSnapshot.Capture(near), "the floor moved with the camera");
    }

    [Test]
    public void TheActorsAreDrawnOnTopOfTheTiles()
    {
        TestWorld world = World("tiles:\n.M...\n#####\n");
        var withPlayer = new FrameBuffer(64, 48);
        Draw(withPlayer, world, Camera(withPlayer, world.Player.Position));

        var withoutPlayer = new FrameBuffer(64, 48);
        world.Player.Body.Position = new Vector2(-100.0f, -100.0f);
        Draw(withoutPlayer, world, Camera(withoutPlayer, new Vector2(20.0f, 8.0f)));

        Assert.AreNotEqual(AsciiSnapshot.Capture(withoutPlayer), AsciiSnapshot.Capture(withPlayer));
    }

    [Test]
    public void ItRefusesToBeBuiltOrDrawnWithoutWhatItNeeds()
    {
        var frame = new FrameBuffer(16, 16);
        TestWorld world = World("tiles:\n.M.\n###\n");
        LevelView view = View();
        Assert.Throws<ArgumentNullException>(() => new LevelView(null!));
        Assert.Throws<ArgumentNullException>(() => view.Draw(null!, Camera(frame, Vector2.Zero), world.World));
        Assert.Throws<ArgumentNullException>(() => view.Draw(frame, null!, world.World));
        Assert.Throws<ArgumentNullException>(() => view.Draw(frame, Camera(frame, Vector2.Zero), null!));
    }

    private static TestWorld World(string level) => new(level);

    private static LevelView View()
    {
        var atlas = new SpriteAtlas(new EmbeddedAssetSource(typeof(MarioGame).Assembly));
        atlas.LoadAll();
        return new LevelView(atlas);
    }

    private static void Draw(FrameBuffer frame, TestWorld world, Camera2D camera) =>
        View().Draw(frame, camera, world.World);

    private static Camera2D Camera(FrameBuffer frame, Vector2 lookAt)
    {
        var camera = new Camera2D(new Vector2(frame.Width, frame.Height));
        camera.SnapTo(lookAt);
        return camera;
    }

    private static bool HasSomethingOtherThanSky(FrameBuffer frame)
    {
        for (int y = 0; y < frame.Height; y++)
        {
            for (int x = 0; x < frame.Width; x++)
            {
                if (frame.GetPixel(x, y) != Palette.Sky)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
