using System;
using GEngine.Core.Contracts;
using GEngine.Core.Patterns;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="ServiceRegistry"/>.</summary>
public sealed class ServiceRegistryTests
{
    private ServiceRegistry _registry = new();

    [Setup]
    public void Setup() => _registry = new ServiceRegistry();

    [Test]
    public void ANewRegistry_IsEmpty()
    {
        Assert.AreEqual(0, _registry.Count);
        Assert.IsFalse(_registry.Contains<ILogger>());
    }

    [Test]
    public void Resolve_ReturnsWhatWasRegisteredUnderTheInterface()
    {
        var logger = new MemoryLogger();
        _registry.Register<ILogger>(logger);
        Assert.AreSame(logger, _registry.Resolve<ILogger>());
        Assert.IsTrue(_registry.Contains<ILogger>());
    }

    [Test]
    public void Resolve_OfAMissingService_NamesTheTypeItWanted()
    {
        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(
            () => _registry.Resolve<ILogger>());
        Assert.IsTrue(failure.Message.Contains(nameof(ILogger), StringComparison.Ordinal));
    }

    [Test]
    public void TryResolve_ReportsAbsenceInsteadOfThrowing()
    {
        Assert.IsFalse(_registry.TryResolve(out ILogger? missing));
        Assert.IsNull(missing);
        _registry.Register<ILogger>(NullLogger.Instance);
        Assert.IsTrue(_registry.TryResolve(out ILogger? found));
        Assert.AreSame(NullLogger.Instance, found);
    }

    [Test]
    public void Register_Twice_ReplacesTheFirstInstance()
    {
        var second = new MemoryLogger();
        _registry.Register<ILogger>(new MemoryLogger());
        _registry.Register<ILogger>(second);
        Assert.AreEqual(1, _registry.Count);
        Assert.AreSame(second, _registry.Resolve<ILogger>());
    }

    [Test]
    public void Unregister_RemovesTheServiceAndSaysWhetherThereWasOne()
    {
        _registry.Register<ILogger>(NullLogger.Instance);
        Assert.IsTrue(_registry.Unregister<ILogger>());
        Assert.IsFalse(_registry.Unregister<ILogger>());
    }

    [Test]
    public void Clear_ForgetsEverything()
    {
        _registry.Register<ILogger>(NullLogger.Instance);
        _registry.Clear();
        Assert.AreEqual(0, _registry.Count);
    }
}
