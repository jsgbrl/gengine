using System;
using GEngine.Core.Scenes;
using GEngine.Core.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="Entity"/>.</summary>
public sealed class EntityTests
{
    [Test]
    public void ANewEntity_HasANameATransformAndNoComponents()
    {
        var entity = new Entity("goomba");
        Assert.AreEqual("goomba", entity.Name);
        Assert.IsNotNull(entity.Transform);
        Assert.AreEqual(0, entity.Components.Count);
        Assert.IsTrue(entity.IsActive);
        Assert.IsNull(entity.Scene);
    }

    [Test]
    public void Add_ReturnsTheComponentSoTheCallerCanKeepIt()
    {
        var entity = new Entity("goomba");
        var component = new CountingComponent();
        Assert.AreSame(component, entity.Add(component));
        Assert.AreEqual(1, entity.Components.Count);
    }

    [Test]
    public void Get_FindsAComponentByType_AndReturnsNullWhenThereIsNone()
    {
        var entity = new Entity("goomba");
        Assert.IsNull(entity.Get<CountingComponent>());
        CountingComponent added = entity.Add(new CountingComponent());
        Assert.AreSame(added, entity.Get<CountingComponent>());
    }

    [Test]
    public void Get_ReturnsTheFirstOfTwoComponentsOfTheSameType()
    {
        var entity = new Entity("goomba");
        CountingComponent first = entity.Add(new CountingComponent());
        entity.Add(new CountingComponent());
        Assert.AreSame(first, entity.Get<CountingComponent>());
    }

    [Test]
    public void Require_FailsWithANameWhenTheComponentIsMissing()
    {
        var entity = new Entity("goomba");
        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(
            () => entity.Require<CountingComponent>());
        Assert.IsTrue(failure.Message.Contains("goomba", StringComparison.Ordinal));
        Assert.IsTrue(failure.Message.Contains(nameof(CountingComponent), StringComparison.Ordinal));
    }

    [Test]
    public void Remove_TakesTheFirstMatchAndReportsWhetherThereWasOne()
    {
        var entity = new Entity("goomba");
        entity.Add(new CountingComponent());
        Assert.IsTrue(entity.Remove<CountingComponent>());
        Assert.IsFalse(entity.Remove<CountingComponent>());
        Assert.AreEqual(0, entity.Components.Count);
    }

    [Test]
    public void AnInactiveEntity_IsSkippedByItsScene()
    {
        var entity = new Entity("goomba") { IsActive = false };
        CountingComponent component = entity.Add(new CountingComponent());
        var scene = new Scene("test");
        scene.Add(entity);
        scene.Update(0.1f);
        Assert.AreEqual(0, component.UpdateCount);
    }
}
