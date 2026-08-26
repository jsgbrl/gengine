using GEngine.Core;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="CollisionListener"/>, the base with six empty callbacks.</summary>
public sealed class CollisionListenerTests
{
    [Test]
    public void TheBaseClass_AcceptsEveryCallbackAndDoesNothing()
    {
        ICollisionListener listener = new SilentListener();
        var bodies = new BodySet();
        var contact = new Contact(bodies.AddBoxAt(0.0f, 0.0f), bodies.AddBoxAt(1.0f, 0.0f), Vector2.UnitX, 0.0f);

        listener.OnCollisionEnter(contact);
        listener.OnCollisionStay(contact);
        listener.OnCollisionExit(contact);
        listener.OnTriggerEnter(contact);
        listener.OnTriggerStay(contact);
        listener.OnTriggerExit(contact);

        Assert.IsNotNull(listener);
    }

    [Test]
    public void ASubclassOverridesOnlyWhatItCaresAbout()
    {
        var listener = new RecordingListener();
        var bodies = new BodySet();
        var contact = new Contact(bodies.AddBoxAt(0.0f, 0.0f), bodies.AddBoxAt(1.0f, 0.0f), Vector2.UnitX, 0.0f);

        listener.OnCollisionEnter(contact);
        Assert.AreEqual(1, listener.CollisionEnterCount);
        Assert.AreEqual(0, listener.CollisionStayCount);
    }

    private sealed class SilentListener : CollisionListener
    {
    }
}
