using GEngine.Core;
using GEngine.Physics.NarrowPhase;
using GEngine.Physics.Tiles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="TileSweep"/>.</summary>
public sealed class TileSweepTests
{
    private TileCollisionSource _tiles = new(20, 10, 16.0f);

    [Setup]
    public void Setup() => _tiles = new TileCollisionSource(20, 10, 16.0f);

    [Test]
    public void AnEmptyMap_StopsNothing()
    {
        SweepResult result = TileSweep.FindNearest(_tiles, Box(8.0f, 8.0f), new Vector2(200.0f, 0.0f), out _);
        Assert.IsFalse(result.IsHit);
    }

    [Test]
    public void ASolidTile_StopsAMovementAndReportsItsBox()
    {
        _tiles.Set(5, 0, TileCollision.Solid);
        SweepResult result = TileSweep.FindNearest(_tiles, Box(8.0f, 8.0f), new Vector2(200.0f, 0.0f), out Aabb tile);
        Assert.IsTrue(result.IsHit);
        Assert.AreEqual(new Vector2(80.0f, 0.0f), tile.Min);
        Assert.AreEqual(new Vector2(-1.0f, 0.0f), result.Normal);
    }

    [Test]
    public void TheNearestSolidTileWins_NotTheFirstOneVisited()
    {
        _tiles.Set(9, 0, TileCollision.Solid);
        _tiles.Set(3, 0, TileCollision.Solid);
        TileSweep.FindNearest(_tiles, Box(8.0f, 8.0f), new Vector2(300.0f, 0.0f), out Aabb tile);
        Assert.AreEqual(48.0f, tile.Min.X);
    }

    [Test]
    public void AOneWayTile_StopsABodyThatIsFallingOntoIt()
    {
        _tiles.Set(0, 4, TileCollision.OneWay);
        SweepResult result = TileSweep.FindNearest(_tiles, Box(8.0f, 50.0f), new Vector2(0.0f, 40.0f), out _);
        Assert.IsTrue(result.IsHit);
        Assert.AreEqual(new Vector2(0.0f, -1.0f), result.Normal);
    }

    [Test]
    public void AOneWayTile_LetsABodyRiseThroughIt()
    {
        _tiles.Set(0, 4, TileCollision.OneWay);
        SweepResult result = TileSweep.FindNearest(_tiles, Box(8.0f, 90.0f), new Vector2(0.0f, -60.0f), out _);
        Assert.IsFalse(result.IsHit);
    }

    [Test]
    public void AOneWayTile_LetsABodyThatIsAlreadyInsideItKeepFalling()
    {
        _tiles.Set(0, 4, TileCollision.OneWay);
        SweepResult result = TileSweep.FindNearest(_tiles, Box(8.0f, 68.0f), new Vector2(0.0f, 10.0f), out _);
        Assert.IsFalse(result.IsHit);
    }

    [Test]
    public void AFastMovement_DoesNotSkipOverATileItPassesThrough()
    {
        _tiles.Set(10, 0, TileCollision.Solid);
        SweepResult result = TileSweep.FindNearest(_tiles, Box(8.0f, 8.0f), new Vector2(10000.0f, 0.0f), out _);
        Assert.IsTrue(result.IsHit);
    }

    [Test]
    public void TryFindOverlap_FindsASolidTileTheBoxIsInside()
    {
        _tiles.Set(2, 2, TileCollision.Solid);
        Assert.IsTrue(TileSweep.TryFindOverlap(_tiles, Box(36.0f, 36.0f), out Aabb tile));
        Assert.AreEqual(new Vector2(32.0f, 32.0f), tile.Min);
    }

    [Test]
    public void TryFindOverlap_IgnoresOneWayTiles()
    {
        _tiles.Set(2, 2, TileCollision.OneWay);
        Assert.IsFalse(TileSweep.TryFindOverlap(_tiles, Box(36.0f, 36.0f), out _));
    }

    [Test]
    public void TryFindOverlap_IgnoresATileTheBoxOnlyTouches()
    {
        _tiles.Set(2, 2, TileCollision.Solid);
        Assert.IsFalse(TileSweep.TryFindOverlap(_tiles, Box(27.0f, 36.0f), out _));
    }

    // A ten by ten box centred at the given point.
    private static Aabb Box(float x, float y) => Aabb.FromCenterSize(new Vector2(x, y), new Vector2(10.0f, 10.0f));
}
