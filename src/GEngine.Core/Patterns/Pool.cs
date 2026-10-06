// Object Pool.
//
// Sixty frames a second times one allocation per spawned coin is three thousand six hundred
// objects a minute for the collector to trace and free. None of them is large, and that is the
// point: the cost is not the memory, it is the pause. A pool trades a little permanent memory
// for a game that never stutters on a generation-zero collection in the middle of a jump.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Patterns;

/// <summary>Hands out reusable instances instead of allocating new ones.</summary>
/// <typeparam name="TItem">Type of the pooled instance.</typeparam>
public sealed class Pool<TItem>
    where TItem : class
{
    private readonly Stack<TItem> _available;
    private readonly Func<TItem> _create;
    private readonly Action<TItem> _reset;

    /// <summary>Creates a pool.</summary>
    /// <param name="create">Makes a fresh instance when the pool runs dry.</param>
    /// <param name="reset">Puts a returned instance back into a usable state.</param>
    /// <param name="initialCapacity">How many instances to make up front.</param>
    public Pool(Func<TItem> create, Action<TItem> reset, int initialCapacity = 0)
    {
        ArgumentNullException.ThrowIfNull(create);
        ArgumentNullException.ThrowIfNull(reset);
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);
        _create = create;
        _reset = reset;
        _available = new Stack<TItem>(initialCapacity);
        Prewarm(initialCapacity);
    }

    /// <summary>How many instances are sitting in the pool.</summary>
    public int AvailableCount => _available.Count;

    /// <summary>How many instances are currently out on loan.</summary>
    public int RentedCount { get; private set; }

    /// <summary>How many instances the pool has ever created.</summary>
    public int CreatedCount { get; private set; }

    /// <summary>Fills the pool with ready instances.</summary>
    /// <param name="count">How many to add.</param>
    public void Prewarm(int count)
    {
        for (int index = 0; index < count; index++)
        {
            _available.Push(CreateOne());
        }
    }

    /// <summary>Takes an instance, creating one only when the pool is empty.</summary>
    /// <returns>A reset, ready-to-use instance.</returns>
    public TItem Rent()
    {
        RentedCount++;
        return _available.Count > 0 ? _available.Pop() : CreateOne();
    }

    /// <summary>Gives an instance back and resets it.</summary>
    /// <param name="item">The instance to return.</param>
    /// <exception cref="InvalidOperationException">More items were returned than were rented.</exception>
    public void Return(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (RentedCount == 0)
        {
            throw new InvalidOperationException("returned an item to a pool that has nothing out on loan");
        }

        RentedCount--;
        _reset(item);
        _available.Push(item);
    }

    private TItem CreateOne()
    {
        CreatedCount++;
        return _create();
    }
}
