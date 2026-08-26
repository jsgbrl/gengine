using System.Collections.Generic;
using GEngine.Core.Patterns;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="EventBus"/>.</summary>
public sealed class EventBusTests
{
    private EventBus _bus = new();
    private List<string> _log = [];

    [Setup]
    public void Setup()
    {
        _bus = new EventBus();
        _log = [];
    }

    [Test]
    public void Publish_WithNoSubscribers_DoesNothing()
    {
        _bus.Publish(new Stomped(1));
        Assert.AreEqual(0, _bus.SubscriberCount<Stomped>());
    }

    [Test]
    public void Publish_ReachesEverySubscriberOfThatType()
    {
        _bus.Subscribe<Stomped>(message => _log.Add("first " + message.Points));
        _bus.Subscribe<Stomped>(message => _log.Add("second " + message.Points));
        _bus.Publish(new Stomped(100));
        Assert.AreEqual(2, _log.Count);
        Assert.AreEqual("first 100", _log[0]);
        Assert.AreEqual("second 100", _log[1]);
    }

    [Test]
    public void Publish_DoesNotReachSubscribersOfAnotherType()
    {
        _bus.Subscribe<Collected>(_ => _log.Add("collected"));
        _bus.Publish(new Stomped(1));
        Assert.AreEqual(0, _log.Count);
    }

    [Test]
    public void Unsubscribe_StopsTheHandlerAndReportsWhetherItWasThere()
    {
        void Handler(Stomped message) => _log.Add("handled");
        _bus.Subscribe<Stomped>(Handler);
        Assert.IsTrue(_bus.Unsubscribe<Stomped>(Handler));
        Assert.IsFalse(_bus.Unsubscribe<Stomped>(Handler));
        _bus.Publish(new Stomped(1));
        Assert.AreEqual(0, _log.Count);
    }

    [Test]
    public void Unsubscribe_OfOneHandler_LeavesTheOthers()
    {
        void First(Stomped message) => _log.Add("first");
        _bus.Subscribe<Stomped>(First);
        _bus.Subscribe<Stomped>(_ => _log.Add("second"));
        _bus.Unsubscribe<Stomped>(First);
        _bus.Publish(new Stomped(1));
        Assert.AreEqual(1, _log.Count);
        Assert.AreEqual("second", _log[0]);
    }

    [Test]
    public void SubscribingFromInsideAHandler_TakesEffectOnTheNextPublish()
    {
        _bus.Subscribe<Stomped>(_ =>
        {
            _log.Add("outer");
            _bus.Subscribe<Stomped>(_ => _log.Add("inner"));
        });
        _bus.Publish(new Stomped(1));
        Assert.AreEqual(1, _log.Count);
        _bus.Publish(new Stomped(2));
        Assert.AreEqual(3, _log.Count);
    }

    [Test]
    public void SubscriberCount_ReportsHowManyHandlersAreListening()
    {
        _bus.Subscribe<Stomped>(_ => { });
        _bus.Subscribe<Stomped>(_ => { });
        Assert.AreEqual(2, _bus.SubscriberCount<Stomped>());
        Assert.AreEqual(0, _bus.SubscriberCount<Collected>());
    }

    [Test]
    public void Clear_ForgetsEverySubscription()
    {
        _bus.Subscribe<Stomped>(_ => _log.Add("handled"));
        _bus.Clear();
        _bus.Publish(new Stomped(1));
        Assert.AreEqual(0, _log.Count);
    }

    private readonly struct Stomped
    {
        public Stomped(int points) => Points = points;

        public int Points { get; }
    }

    private readonly struct Collected
    {
    }
}
