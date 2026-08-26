using System;
using GEngine.Input.Keyboard;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="ConsoleKeyReader"/>. A test cannot press a key on the real keyboard, so
/// what is asserted here is the property that matters when nobody is at one: with input
/// redirected - a pipe, a file, a build server - the reader reports nothing instead of
/// throwing, which is the failure this class exists to prevent.
/// </summary>
public sealed class ConsoleKeyReaderTests
{
    [Test]
    public void ItReportsWhetherAKeyboardCanBeReadAtAll()
    {
        Assert.AreEqual(!Console.IsInputRedirected, ConsoleKeyReader.IsAvailable);
    }

    [Test]
    public void WithInputRedirected_ItReturnsNothingRatherThanThrowing()
    {
        if (ConsoleKeyReader.IsAvailable)
        {
            return;
        }

        Assert.IsFalse(ConsoleKeyReader.Instance.TryReadKey(out ConsoleKey key));
        Assert.AreEqual(default(ConsoleKey), key);
    }

    [Test]
    public void Instance_IsShared_BecauseItHoldsNoState()
    {
        Assert.AreSame(ConsoleKeyReader.Instance, ConsoleKeyReader.Instance);
    }
}
