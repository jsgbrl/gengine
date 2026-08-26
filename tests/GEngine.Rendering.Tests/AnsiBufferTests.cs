using System;
using System.IO;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="AnsiBuffer"/>.</summary>
public sealed class AnsiBufferTests
{
    [Test]
    public void ANewBufferIsEmptyAndHasTheCapacityItWasAskedFor()
    {
        var buffer = new AnsiBuffer(16);
        Assert.AreEqual(0, buffer.Length);
        Assert.AreEqual(16, buffer.Capacity);
    }

    [Test]
    public void ADegenerateCapacityIsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new AnsiBuffer(0));
    }

    [Test]
    public void AppendingCharactersAndStringsBuildsOneRun()
    {
        var buffer = new AnsiBuffer(16);
        buffer.Append('a');
        buffer.Append("bc");
        Assert.AreEqual("abc", buffer.ToString());
        Assert.AreEqual(3, buffer.Length);
    }

    [TestCase(0, "0")]
    [TestCase(7, "7")]
    [TestCase(42, "42")]
    [TestCase(255, "255")]
    [TestCase(1000000, "1000000")]
    public void AppendNumber_WritesDecimalDigits(int value, string expected)
    {
        var buffer = new AnsiBuffer(16);
        buffer.AppendNumber(value);
        Assert.AreEqual(expected, buffer.ToString());
    }

    [Test]
    public void AppendNumber_RefusesANegativeNumber()
    {
        var buffer = new AnsiBuffer(16);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AppendNumber(-1));
    }

    [Test]
    public void TheBufferGrowsRatherThanOverflowing()
    {
        var buffer = new AnsiBuffer(2);
        buffer.Append("0123456789");
        Assert.AreEqual("0123456789", buffer.ToString());
        Assert.IsTrue(buffer.Capacity >= 10);
    }

    [Test]
    public void Clear_EmptiesTheBufferAndKeepsItsMemory()
    {
        var buffer = new AnsiBuffer(64);
        buffer.Append("something");
        buffer.Clear();
        Assert.AreEqual(0, buffer.Length);
        Assert.AreEqual(64, buffer.Capacity);
    }

    [Test]
    public void FlushTo_WritesEverythingAndEmptiesTheBuffer()
    {
        var buffer = new AnsiBuffer(64);
        buffer.Append("frame");
        using var writer = new StringWriter();
        buffer.FlushTo(writer);
        Assert.AreEqual("frame", writer.ToString());
        Assert.AreEqual(0, buffer.Length);
    }

    [Test]
    public void AWarmBufferNeverAllocatesAgain()
    {
        var buffer = new AnsiBuffer(1024);
        for (int round = 0; round < 8; round++)
        {
            Fill(buffer);
            buffer.Clear();
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        Fill(buffer);
        buffer.Clear();
        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    private static void Fill(AnsiBuffer buffer)
    {
        for (int index = 0; index < 50; index++)
        {
            buffer.Append("\u001b[38;2;");
            buffer.AppendNumber(index);
            buffer.Append('m');
        }
    }
}
