// Writes down every callback it receives, so a test can assert that enter fired once, stay
// fired while the touch lasted, and exit fired once at the end.

using System.Collections.Generic;

namespace GEngine.Physics.Tests.Doubles;

internal sealed class RecordingListener : CollisionListener
{
    private readonly List<string> _calls = [];

    public IReadOnlyList<string> Calls => _calls;

    public int CollisionEnterCount { get; private set; }

    public int CollisionStayCount { get; private set; }

    public int CollisionExitCount { get; private set; }

    public int TriggerEnterCount { get; private set; }

    public int TriggerStayCount { get; private set; }

    public int TriggerExitCount { get; private set; }

    public Contact LastContact { get; private set; }

    public override void OnCollisionEnter(Contact contact)
    {
        CollisionEnterCount++;
        Record("collision enter", contact);
    }

    public override void OnCollisionStay(Contact contact)
    {
        CollisionStayCount++;
        Record("collision stay", contact);
    }

    public override void OnCollisionExit(Contact contact)
    {
        CollisionExitCount++;
        Record("collision exit", contact);
    }

    public override void OnTriggerEnter(Contact contact)
    {
        TriggerEnterCount++;
        Record("trigger enter", contact);
    }

    public override void OnTriggerStay(Contact contact)
    {
        TriggerStayCount++;
        Record("trigger stay", contact);
    }

    public override void OnTriggerExit(Contact contact)
    {
        TriggerExitCount++;
        Record("trigger exit", contact);
    }

    private void Record(string what, Contact contact)
    {
        LastContact = contact;
        _calls.Add(what + " " + contact.Other.Id);
    }
}
