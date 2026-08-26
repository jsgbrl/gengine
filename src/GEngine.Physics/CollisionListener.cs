// Template Method again: override the one callback you care about and inherit five empty
// ones. Implementing the interface directly is still allowed and is what some tests do.

namespace GEngine.Physics;

/// <summary>A listener with every callback already written as a no-op.</summary>
public abstract class CollisionListener : ICollisionListener
{
    /// <inheritdoc/>
    public virtual void OnCollisionEnter(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnCollisionStay(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnCollisionExit(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnTriggerEnter(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnTriggerStay(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnTriggerExit(Contact contact)
    {
    }
}
