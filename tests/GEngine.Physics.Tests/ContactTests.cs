using GEngine.Core;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="Contact"/>.</summary>
public sealed class ContactTests
{
    private BodySet _bodies = new();

    [Setup]
    public void Setup() => _bodies = new BodySet();

    [Test]
    public void AContactKeepsBothBodiesTheNormalAndTheDepth()
    {
        RigidBody2D self = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D other = _bodies.AddBoxAt(10.0f, 0.0f);
        var contact = new Contact(self, other, new Vector2(-1.0f, 0.0f), 3.0f);
        Assert.AreSame(self, contact.Self);
        Assert.AreSame(other, contact.Other);
        Assert.AreEqual(new Vector2(-1.0f, 0.0f), contact.Normal);
        Assert.ApproximatelyEqual(3.0f, contact.Penetration);
    }

    [Test]
    public void Flipped_SwapsTheBodiesAndReversesTheNormal()
    {
        RigidBody2D self = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D other = _bodies.AddBoxAt(10.0f, 0.0f);
        Contact flipped = new Contact(self, other, new Vector2(0.0f, -1.0f), 2.0f).Flipped();
        Assert.AreSame(other, flipped.Self);
        Assert.AreSame(self, flipped.Other);
        Assert.AreEqual(new Vector2(0.0f, 1.0f), flipped.Normal);
        Assert.ApproximatelyEqual(2.0f, flipped.Penetration);
    }

    [Test]
    public void FlippingTwice_GivesTheOriginalBack()
    {
        RigidBody2D self = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D other = _bodies.AddBoxAt(10.0f, 0.0f);
        var contact = new Contact(self, other, new Vector2(0.0f, -1.0f), 2.0f);
        Assert.AreEqual(contact, contact.Flipped().Flipped());
    }

    [Test]
    public void Equality_ComparesTheBodiesByReferenceAndTheRestByValue()
    {
        RigidBody2D self = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D other = _bodies.AddBoxAt(10.0f, 0.0f);
        var contact = new Contact(self, other, Vector2.UnitX, 1.0f);
        Assert.IsTrue(contact == new Contact(self, other, Vector2.UnitX, 1.0f));
        Assert.IsTrue(contact != new Contact(self, other, Vector2.UnitY, 1.0f));
        Assert.IsTrue(contact.Equals((object)new Contact(self, other, Vector2.UnitX, 1.0f)));
        Assert.IsFalse(contact.Equals("not a contact"));
        Assert.AreEqual(contact.GetHashCode(), new Contact(self, other, Vector2.UnitX, 1.0f).GetHashCode());
    }

    [Test]
    public void ToString_NamesBothBodiesAndTheFace()
    {
        RigidBody2D self = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D other = _bodies.AddBoxAt(10.0f, 0.0f);
        Assert.AreEqual("1 touched 2 along (1, 0)", new Contact(self, other, Vector2.UnitX, 0.0f).ToString());
    }
}
