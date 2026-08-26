// Phase six: turning a set of touches per step into enter, stay and exit, each fired
// exactly once.
//
// A contact present this step but not last step is an enter; present in both is a stay;
// present last step but not this one is an exit. Two lists and two sets are swapped rather
// than reallocated: the sets answer "was it there?" in constant time, the lists keep the
// callbacks in identity order. Order matters because a game mutates state inside these
// callbacks, and a reproducible simulation cannot depend on hash order.

using System.Collections.Generic;

namespace GEngine.Physics;

/// <content>Dispatch of the collision and trigger callbacks.</content>
public sealed partial class PhysicsWorld
{
    private void DispatchContacts()
    {
        foreach (Contact contact in _currentContacts)
        {
            Notify(contact, _previousKeys.Contains(KeyOf(contact)) ? ContactPhase.Stay : ContactPhase.Enter);
        }

        foreach (Contact contact in _previousContacts)
        {
            if (!_currentKeys.Contains(KeyOf(contact)))
            {
                Notify(contact, ContactPhase.Exit);
            }
        }
    }

    private static void Notify(Contact contact, ContactPhase phase)
    {
        bool isTrigger = contact.Self.IsTrigger || contact.Other.IsTrigger;
        Deliver(contact.Self.Listener, contact, phase, isTrigger);
        Deliver(contact.Other.Listener, contact.Flipped(), phase, isTrigger);
    }

    private static void Deliver(ICollisionListener? listener, Contact contact, ContactPhase phase, bool isTrigger)
    {
        if (listener is null)
        {
            return;
        }

        if (isTrigger)
        {
            DeliverTrigger(listener, contact, phase);
            return;
        }

        DeliverCollision(listener, contact, phase);
    }

    private static void DeliverCollision(ICollisionListener listener, Contact contact, ContactPhase phase)
    {
        switch (phase)
        {
            case ContactPhase.Enter:
                listener.OnCollisionEnter(contact);
                break;
            case ContactPhase.Stay:
                listener.OnCollisionStay(contact);
                break;
            default:
                listener.OnCollisionExit(contact);
                break;
        }
    }

    private static void DeliverTrigger(ICollisionListener listener, Contact contact, ContactPhase phase)
    {
        switch (phase)
        {
            case ContactPhase.Enter:
                listener.OnTriggerEnter(contact);
                break;
            case ContactPhase.Stay:
                listener.OnTriggerStay(contact);
                break;
            default:
                listener.OnTriggerExit(contact);
                break;
        }
    }

    private void ForgetContactsOf(RigidBody2D body)
    {
        Forget(_currentContacts, _currentKeys, body);
        Forget(_previousContacts, _previousKeys, body);
    }

    private static void Forget(List<Contact> contacts, HashSet<long> keys, RigidBody2D body)
    {
        for (int index = contacts.Count - 1; index >= 0; index--)
        {
            Contact contact = contacts[index];
            if (ReferenceEquals(contact.Self, body) || ReferenceEquals(contact.Other, body))
            {
                keys.Remove(KeyOf(contact));
                contacts.RemoveAt(index);
            }
        }
    }

    private enum ContactPhase
    {
        Enter,
        Stay,
        Exit,
    }
}
