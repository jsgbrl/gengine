using System;
using GEngine.Core.Scenes;
using GEngine.Core.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers the Template Method sequence of <see cref="Component"/>.</summary>
public sealed class ComponentTests
{
    [Test]
    public void ADetachedComponent_RefusesToNameAnEntity()
    {
        var component = new CountingComponent();
        Assert.IsFalse(component.IsAttached);
        Assert.Throws<InvalidOperationException>(() => _ = component.Entity);
    }

    [Test]
    public void Attaching_RunsOnAttachExactlyOnce()
    {
        var entity = new Entity("player");
        CountingComponent component = entity.Add(new CountingComponent());
        Assert.IsTrue(component.IsAttached);
        Assert.AreEqual(1, component.AttachCount);
        Assert.AreSame(entity, component.Entity);
        Assert.AreSame(entity.Transform, component.Transform);
    }

    [Test]
    public void Detaching_RunsOnDetachAndForgetsTheEntity()
    {
        var entity = new Entity("player");
        CountingComponent component = entity.Add(new CountingComponent());
        Assert.IsTrue(entity.Remove<CountingComponent>());
        Assert.AreEqual(1, component.DetachCount);
        Assert.IsFalse(component.IsAttached);
    }

    [Test]
    public void ADisabledComponent_StaysAttachedButStopsUpdating()
    {
        var entity = new Entity("player");
        CountingComponent component = entity.Add(new CountingComponent());
        component.IsEnabled = false;
        var scene = new Scene("test");
        scene.Add(entity);
        scene.Update(0.1f);
        scene.FixedUpdate(0.1f);
        Assert.AreEqual(0, component.UpdateCount);
        Assert.AreEqual(0, component.FixedUpdateCount);
        Assert.IsTrue(component.IsAttached);
    }

    [Test]
    public void AnEnabledComponent_ReceivesBothKindsOfUpdate()
    {
        var entity = new Entity("player");
        CountingComponent component = entity.Add(new CountingComponent());
        var scene = new Scene("test");
        scene.Add(entity);
        scene.Update(0.1f);
        scene.FixedUpdate(0.1f);
        Assert.AreEqual(1, component.UpdateCount);
        Assert.AreEqual(1, component.FixedUpdateCount);
    }
}
