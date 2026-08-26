using System;
using GEngine.Core.Patterns;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="Pool{TItem}"/>.</summary>
public sealed class PoolTests
{
    [Test]
    public void ANewPool_CreatesItsInitialCapacityUpFront()
    {
        Pool<Box> pool = Make(initialCapacity: 3);
        Assert.AreEqual(3, pool.AvailableCount);
        Assert.AreEqual(3, pool.CreatedCount);
        Assert.AreEqual(0, pool.RentedCount);
    }

    [Test]
    public void Rent_TakesFromThePoolWithoutCreatingAnything()
    {
        Pool<Box> pool = Make(initialCapacity: 1);
        pool.Rent();
        Assert.AreEqual(0, pool.AvailableCount);
        Assert.AreEqual(1, pool.RentedCount);
        Assert.AreEqual(1, pool.CreatedCount);
    }

    [Test]
    public void Rent_FromAnEmptyPool_CreatesOneMore()
    {
        Pool<Box> pool = Make(initialCapacity: 0);
        pool.Rent();
        Assert.AreEqual(1, pool.CreatedCount);
    }

    [Test]
    public void Return_ResetsTheItemBeforeItGoesBackOnTheShelf()
    {
        Pool<Box> pool = Make(initialCapacity: 1);
        Box box = pool.Rent();
        box.Value = 42;
        pool.Return(box);
        Assert.AreEqual(0, box.Value);
        Assert.AreEqual(1, pool.AvailableCount);
        Assert.AreEqual(0, pool.RentedCount);
    }

    [Test]
    public void RentAndReturn_InALoop_NeverCreateAnotherInstance()
    {
        Pool<Box> pool = Make(initialCapacity: 1);
        for (int round = 0; round < 100; round++)
        {
            pool.Return(pool.Rent());
        }

        Assert.AreEqual(1, pool.CreatedCount);
    }

    [Test]
    public void Return_OfSomethingNeverRented_IsRefused()
    {
        Pool<Box> pool = Make(initialCapacity: 1);
        Assert.Throws<InvalidOperationException>(() => pool.Return(new Box()));
    }

    [Test]
    public void Prewarm_AddsMoreReadyInstances()
    {
        Pool<Box> pool = Make(initialCapacity: 0);
        pool.Prewarm(5);
        Assert.AreEqual(5, pool.AvailableCount);
    }

    [Test]
    public void ANegativeCapacity_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Make(initialCapacity: -1));
    }

    private static Pool<Box> Make(int initialCapacity)
    {
        return new Pool<Box>(static () => new Box(), static box => box.Value = 0, initialCapacity);
    }

    private sealed class Box
    {
        public int Value { get; set; }
    }
}
