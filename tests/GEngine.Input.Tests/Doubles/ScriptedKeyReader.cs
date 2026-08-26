// A keyboard nobody has to touch. Keys are queued and handed out one poll at a time, exactly
// the way a terminal delivers them.

using System;
using System.Collections.Generic;
using GEngine.Input.Keyboard;

namespace GEngine.Input.Tests.Doubles;

internal sealed class ScriptedKeyReader : IKeyReader
{
    private readonly Queue<ConsoleKey> _pending = new();

    public int ReadCount { get; private set; }

    public ScriptedKeyReader Press(params ConsoleKey[] keys)
    {
        foreach (ConsoleKey key in keys)
        {
            _pending.Enqueue(key);
        }

        return this;
    }

    public bool TryReadKey(out ConsoleKey key)
    {
        if (_pending.Count == 0)
        {
            key = default;
            return false;
        }

        ReadCount++;
        key = _pending.Dequeue();
        return true;
    }
}
