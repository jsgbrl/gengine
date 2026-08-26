using GEngine.Core.Contracts;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="MemoryLogger"/>.</summary>
public sealed class MemoryLoggerTests
{
    [Test]
    public void EveryLevel_IsKeptInOrderAndTagged()
    {
        var logger = new MemoryLogger();
        logger.Info("one");
        logger.Warning("two");
        logger.Error("three");
        Assert.AreEqual(3, logger.Messages.Count);
        Assert.AreEqual("info: one", logger.Messages[0]);
        Assert.AreEqual("warning: two", logger.Messages[1]);
        Assert.AreEqual("error: three", logger.Messages[2]);
    }

    [Test]
    public void Contains_FindsTextInsideAnyMessage()
    {
        var logger = new MemoryLogger();
        logger.Warning("degrading to 256 colours");
        Assert.IsTrue(logger.Contains("256"));
        Assert.IsFalse(logger.Contains("truecolor"));
    }

    [Test]
    public void Clear_ForgetsEveryMessage()
    {
        var logger = new MemoryLogger();
        logger.Info("one");
        logger.Clear();
        Assert.AreEqual(0, logger.Messages.Count);
    }
}
