using System;
using GEngine.Core.Scenes;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="Transform2D"/> and its parent-child arithmetic.</summary>
public sealed class Transform2DTests
{
    [Test]
    public void ARootTransform_HasTheSameLocalAndWorldPosition()
    {
        var transform = new Transform2D { LocalPosition = new Vector2(3.0f, 4.0f) };
        Assert.AreEqual(new Vector2(3.0f, 4.0f), transform.Position);
        Assert.IsNull(transform.Parent);
    }

    [Test]
    public void AChild_AddsItsParentPosition()
    {
        var parent = new Transform2D { LocalPosition = new Vector2(10.0f, 0.0f) };
        var child = new Transform2D { LocalPosition = new Vector2(1.0f, 2.0f) };
        parent.AddChild(child);
        Assert.AreEqual(new Vector2(11.0f, 2.0f), child.Position);
    }

    [Test]
    public void AChild_MultipliesItsParentScale()
    {
        var parent = new Transform2D { LocalScale = new Vector2(2.0f, 3.0f) };
        var child = new Transform2D { LocalScale = new Vector2(4.0f, 5.0f) };
        parent.AddChild(child);
        Assert.AreEqual(new Vector2(8.0f, 15.0f), child.Scale);
    }

    [Test]
    public void SettingTheWorldPosition_RewritesTheLocalOne()
    {
        var parent = new Transform2D { LocalPosition = new Vector2(10.0f, 10.0f) };
        var child = new Transform2D();
        parent.AddChild(child);
        child.Position = new Vector2(12.0f, 13.0f);
        Assert.AreEqual(new Vector2(2.0f, 3.0f), child.LocalPosition);
    }

    [Test]
    public void ToWorld_AndToLocal_AreInverses()
    {
        var parent = new Transform2D { LocalPosition = new Vector2(5.0f, -5.0f), LocalScale = new Vector2(2.0f, 2.0f) };
        var child = new Transform2D { LocalPosition = new Vector2(1.0f, 1.0f) };
        parent.AddChild(child);
        var point = new Vector2(3.0f, 7.0f);
        Assert.IsTrue(child.ToLocal(child.ToWorld(point)).ApproximatelyEquals(point));
    }

    [Test]
    public void AddChild_MovesAChildFromItsPreviousParent()
    {
        var first = new Transform2D();
        var second = new Transform2D();
        var child = new Transform2D();
        first.AddChild(child);
        second.AddChild(child);
        Assert.AreEqual(0, first.Children.Count);
        Assert.AreEqual(1, second.Children.Count);
        Assert.AreSame(second, child.Parent);
    }

    [Test]
    public void RemoveChild_MakesTheChildARootAgain()
    {
        var parent = new Transform2D { LocalPosition = new Vector2(10.0f, 0.0f) };
        var child = new Transform2D { LocalPosition = new Vector2(1.0f, 0.0f) };
        parent.AddChild(child);
        Assert.IsTrue(parent.RemoveChild(child));
        Assert.IsNull(child.Parent);
        Assert.AreEqual(new Vector2(1.0f, 0.0f), child.Position);
    }

    [Test]
    public void RemoveChild_OfSomeoneElsesChild_ChangesNothing()
    {
        var parent = new Transform2D();
        var stranger = new Transform2D();
        Assert.IsFalse(parent.RemoveChild(stranger));
    }

    [Test]
    public void ATransform_CannotBecomeItsOwnAncestor()
    {
        var parent = new Transform2D();
        var child = new Transform2D();
        parent.AddChild(child);
        Assert.Throws<ArgumentException>(() => child.AddChild(parent));
        Assert.Throws<ArgumentException>(() => parent.AddChild(parent));
    }
}
