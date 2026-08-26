using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="Camera2D"/>: the dead zone, the clamp and the one-way scroll.</summary>
public sealed class Camera2DTests
{
    private static readonly Vector2 ViewSize = new(100.0f, 60.0f);

    private Camera2D _camera = new(ViewSize);

    [Setup]
    public void Setup() => _camera = new Camera2D(ViewSize) { DeadZone = new Vector2(20.0f, 10.0f) };

    [Test]
    public void ANewCameraSitsAtTheOriginAndSeesTheViewSize()
    {
        Assert.AreEqual(Vector2.Zero, _camera.Position);
        Assert.AreEqual(ViewSize, _camera.ViewSize);
        Assert.AreEqual(new Vector2(100.0f, 60.0f), _camera.View.Max);
    }

    [Test]
    public void ATargetInsideTheDeadZone_DoesNotMoveTheCameraAtAll()
    {
        _camera.Follow(new Vector2(60.0f, 32.0f));
        Assert.AreEqual(Vector2.Zero, _camera.Position);
    }

    [Test]
    public void ATargetLeavingTheDeadZone_MovesTheCameraByExactlyTheExcess()
    {
        _camera.Follow(new Vector2(75.0f, 30.0f));
        Assert.ApproximatelyEqual(5.0f, _camera.Position.X);
        Assert.ApproximatelyEqual(0.0f, _camera.Position.Y);
    }

    [Test]
    public void FollowingTheSameTargetTwice_MovesTheCameraOnlyOnce()
    {
        var target = new Vector2(75.0f, 30.0f);
        _camera.Follow(target);
        Vector2 afterFirst = _camera.Position;
        _camera.Follow(target);
        Assert.AreEqual(afterFirst, _camera.Position, "the target is inside the dead zone again");
    }

    [Test]
    public void SnapTo_CentresTheViewAtOnce()
    {
        _camera.SnapTo(new Vector2(500.0f, 200.0f));
        Assert.ApproximatelyEqual(450.0f, _camera.Position.X);
        Assert.ApproximatelyEqual(170.0f, _camera.Position.Y);
    }

    [Test]
    public void TheLevelBoundsClampTheView()
    {
        _camera.LevelBounds = new Aabb(Vector2.Zero, new Vector2(300.0f, 60.0f));
        _camera.SnapTo(new Vector2(1000.0f, 30.0f));
        Assert.ApproximatelyEqual(200.0f, _camera.Position.X, 0.01f, "the right edge of the level");
        _camera.SnapTo(new Vector2(-1000.0f, 30.0f));
        Assert.ApproximatelyEqual(0.0f, _camera.Position.X, 0.01f);
    }

    [Test]
    public void ALevelNarrowerThanTheView_PinsTheCameraAtItsStart()
    {
        _camera.LevelBounds = new Aabb(Vector2.Zero, new Vector2(50.0f, 30.0f));
        _camera.SnapTo(new Vector2(1000.0f, 1000.0f));
        Assert.AreEqual(Vector2.Zero, _camera.Position);
    }

    [Test]
    public void NoLevelBounds_MeansNoClamp()
    {
        _camera.SnapTo(new Vector2(-1000.0f, -1000.0f));
        Assert.ApproximatelyEqual(-1050.0f, _camera.Position.X);
    }

    [Test]
    public void AForwardOnlyCamera_NeverGoesBack()
    {
        _camera.Scroll = CameraScroll.ForwardOnly;
        _camera.Follow(new Vector2(200.0f, 30.0f));
        float furthest = _camera.Position.X;
        Assert.IsTrue(furthest > 0.0f);

        _camera.Follow(new Vector2(0.0f, 30.0f));
        Assert.ApproximatelyEqual(furthest, _camera.Position.X, 0.01f);
    }

    [Test]
    public void AForwardOnlyCamera_StillFollowsVertically()
    {
        _camera.Scroll = CameraScroll.ForwardOnly;
        _camera.Follow(new Vector2(50.0f, 200.0f));
        Assert.IsTrue(_camera.Position.Y > 0.0f);
    }

    [Test]
    public void WorldToScreen_AndScreenToWorld_AreInverses()
    {
        _camera.SnapTo(new Vector2(500.0f, 200.0f));
        var world = new Vector2(512.0f, 205.0f);
        Assert.IsTrue(_camera.ScreenToWorld(_camera.WorldToScreen(world)).ApproximatelyEquals(world));
        Assert.AreEqual(new Vector2(62.0f, 35.0f), _camera.WorldToScreen(world));
    }

    [Test]
    public void IsVisible_AnswersWhetherSomethingIsWorthDrawing()
    {
        Assert.IsTrue(_camera.IsVisible(Aabb.FromCenterSize(new Vector2(50.0f, 30.0f), Vector2.One)));
        Assert.IsFalse(_camera.IsVisible(Aabb.FromCenterSize(new Vector2(5000.0f, 30.0f), Vector2.One)));
    }
}
