using GEngine.Core.Contracts;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="NullLogger"/>.</summary>
public sealed class NullLoggerTests
{
    [Test]
    public void EveryMethod_AcceptsAMessageAndDiscardsIt()
    {
        NullLogger logger = NullLogger.Instance;
        logger.Info("one");
        logger.Warning("two");
        logger.Error("three");
        Assert.IsNotNull(logger);
    }

    [Test]
    public void Instance_IsShared_BecauseItHoldsNoState()
    {
        Assert.AreSame(NullLogger.Instance, NullLogger.Instance);
    }
}
