using System;
using GEngine.Core;
using GEngine.Physics.Tiles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="TileCollisionSource"/>.</summary>
public sealed class TileCollisionSourceTests
{
    private TileCollisionSource _tiles = new(4, 3, 16.0f);

    [Setup]
    public void Setup() => _tiles = new TileCollisionSource(4, 3, 16.0f);

    [Test]
    public void ANewMapIsEmpty()
    {
        Assert.AreEqual(TileCollision.None, _tiles.At(0, 0));
        Assert.AreEqual(4, _tiles.Columns);
        Assert.AreEqual(3, _tiles.Rows);
        Assert.ApproximatelyEqual(16.0f, _tiles.TileSize);
    }

    [Test]
    public void SetAndAt_AgreeAboutEveryTile()
    {
        _tiles.Set(2, 1, TileCollision.Solid);
        Assert.AreEqual(TileCollision.Solid, _tiles.At(2, 1));
        Assert.AreEqual(TileCollision.None, _tiles.At(1, 2), "row and column are not swapped");
    }

    [Test]
    public void OutsideTheMap_IsAlwaysEmptyRatherThanAnError()
    {
        Assert.AreEqual(TileCollision.None, _tiles.At(-1, 0));
        Assert.AreEqual(TileCollision.None, _tiles.At(0, -1));
        Assert.AreEqual(TileCollision.None, _tiles.At(4, 0));
        Assert.AreEqual(TileCollision.None, _tiles.At(0, 3));
    }

    [Test]
    public void Set_OutsideTheMap_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _tiles.Set(4, 0, TileCollision.Solid));
    }

    [Test]
    public void BoundsOf_PutsTileZeroZeroAtTheOrigin()
    {
        Aabb bounds = _tiles.BoundsOf(0, 0);
        Assert.AreEqual(Vector2.Zero, bounds.Min);
        Assert.AreEqual(new Vector2(16.0f, 16.0f), bounds.Max);
    }

    [Test]
    public void BoundsOf_AdvancesByOneTilePerColumnAndRow()
    {
        Aabb bounds = _tiles.BoundsOf(2, 1);
        Assert.AreEqual(new Vector2(32.0f, 16.0f), bounds.Min);
        Assert.AreEqual(new Vector2(48.0f, 32.0f), bounds.Max);
    }

    [Test]
    public void ColumnAtAndRowAt_FloorTowardsNegativeInfinity()
    {
        Assert.AreEqual(0, _tiles.ColumnAt(15.9f));
        Assert.AreEqual(1, _tiles.ColumnAt(16.0f));
        Assert.AreEqual(-1, _tiles.ColumnAt(-0.1f));
        Assert.AreEqual(2, _tiles.RowAt(33.0f));
    }

    [Test]
    public void Bounds_CoverTheWholeMap()
    {
        Assert.AreEqual(new Vector2(64.0f, 48.0f), _tiles.Bounds.Max);
    }

    [Test]
    public void Contains_KnowsWhichPositionsExist()
    {
        Assert.IsTrue(_tiles.Contains(3, 2));
        Assert.IsFalse(_tiles.Contains(4, 2));
    }

    [Test]
    public void ADegenerateMap_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new TileCollisionSource(0, 1, 16.0f));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new TileCollisionSource(1, 0, 16.0f));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new TileCollisionSource(1, 1, 0.0f));
    }
}
