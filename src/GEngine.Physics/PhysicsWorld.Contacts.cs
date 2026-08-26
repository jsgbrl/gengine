// Phases four and five: dynamic bodies against each other, and triggers.
//
// Two moving boxes have no privileged axis, so this half of the solver is discrete rather
// than swept: measure the overlap, push the two apart in inverse proportion to their
// masses, and exchange an impulse along the contact normal. The enter, stay and exit
// bookkeeping those contacts feed is next door in PhysicsWorld.Events.cs.

using System;
using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.NarrowPhase;

namespace GEngine.Physics;

/// <content>Contact collection and impulse resolution.</content>
public sealed partial class PhysicsWorld
{
    private List<Contact> _currentContacts = [];
    private List<Contact> _previousContacts = [];
    private HashSet<long> _currentKeys = [];
    private HashSet<long> _previousKeys = [];

    /// <summary>How many pairs are touching right now.</summary>
    public int ContactCount => _currentContacts.Count;

    private static long KeyOf(Contact contact) => ((long)contact.Self.Id << 32) | (uint)contact.Other.Id;

    private void SwapContactBuffers()
    {
        (_previousContacts, _currentContacts) = (_currentContacts, _previousContacts);
        (_previousKeys, _currentKeys) = (_currentKeys, _previousKeys);
        _currentContacts.Clear();
        _currentKeys.Clear();
    }

    // Contacts are stored once per pair, always seen from the body with the lower identity.
    // Storing both directions would fire every callback twice; storing whichever came first
    // would make the normal depend on iteration order.
    private void AddContact(RigidBody2D self, RigidBody2D other, Vector2 normal, float penetration)
    {
        var pair = BroadPhasePair.Ordered(self, other);
        if (!_currentKeys.Add(pair.Key))
        {
            return;
        }

        _currentContacts.Add(ReferenceEquals(pair.First, self)
            ? new Contact(self, other, normal, penetration)
            : new Contact(other, self, -normal, penetration));
    }

    // A one-way platform is deliberately left out. Being inside one is the legal state of a
    // body on its way up through a ledge, and the discrete solver has no way to tell that
    // from a body that has sunk into a floor: it would see an overlap and push. Everything
    // about one-way platforms is decided by the swept pass, which does know which way the
    // body is going.
    private void ResolvePairs()
    {
        foreach (BroadPhasePair pair in _pairs)
        {
            if (pair.First.IsOneWay || pair.Second.IsOneWay)
            {
                continue;
            }

            if (!AabbCollision.TryGetMinimumTranslation(pair.First.Bounds, pair.Second.Bounds, out Vector2 push))
            {
                continue;
            }

            AddContact(pair.First, pair.Second, push.Normalized(), push.Length);
            if (!pair.First.IsTrigger && !pair.Second.IsTrigger)
            {
                Resolve(pair.First, pair.Second, push);
            }
        }
    }

    private void Resolve(RigidBody2D first, RigidBody2D second, Vector2 push)
    {
        float total = first.InverseMass + second.InverseMass;
        if (total <= 0.0f)
        {
            return;
        }

        first.Position += push * (first.InverseMass / total);
        second.Position -= push * (second.InverseMass / total);
        ApplyImpulse(first, second, -push.Normalized(), total);
    }

    private void ApplyImpulse(RigidBody2D first, RigidBody2D second, Vector2 normal, float inverseMassSum)
    {
        float approach = Vector2.Dot(second.Velocity - first.Velocity, normal);
        if (approach >= 0.0f)
        {
            return;
        }

        float restitution = MathF.Max(first.Restitution, second.Restitution);
        float bounce = -approach > _settings.RestingSpeedPixelsPerSecond ? restitution : 0.0f;
        float magnitude = -(1.0f + bounce) * approach / inverseMassSum;
        first.Velocity -= normal * (magnitude * first.InverseMass);
        second.Velocity += normal * (magnitude * second.InverseMass);
        ApplyFriction(first, second, normal, magnitude);
    }

    // Coulomb friction: the sideways impulse is whatever it takes to stop the sliding, but
    // never more than the coefficient times the impulse that stopped the approach.
    private static void ApplyFriction(RigidBody2D first, RigidBody2D second, Vector2 normal, float magnitude)
    {
        float friction = MathF.Sqrt(first.Friction * second.Friction);
        if (friction <= 0.0f)
        {
            return;
        }

        Vector2 relative = second.Velocity - first.Velocity;
        Vector2 tangent = (relative - (normal * Vector2.Dot(relative, normal))).Normalized();
        float sliding = Vector2.Dot(relative, tangent);
        float total = first.InverseMass + second.InverseMass;
        float impulse = MathG.Clamp(-sliding / total, -friction * magnitude, friction * magnitude);
        first.Velocity -= tangent * (impulse * first.InverseMass);
        second.Velocity += tangent * (impulse * second.InverseMass);
    }
}
