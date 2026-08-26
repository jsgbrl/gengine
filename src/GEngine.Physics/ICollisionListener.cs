// Six callbacks on one interface instead of six delegates on every body: one reference per
// body, no allocation when nobody is listening, and the enter/stay/exit trio stays together
// where a reader can see that it is a trio.

namespace GEngine.Physics;

/// <summary>Receives the collision and trigger callbacks of one body.</summary>
public interface ICollisionListener
{
    /// <summary>The first step in which the two bodies touch.</summary>
    /// <param name="contact">What was touched, and from which side.</param>
    void OnCollisionEnter(Contact contact);

    /// <summary>Every step after the first in which they are still touching.</summary>
    /// <param name="contact">What is being touched, and from which side.</param>
    void OnCollisionStay(Contact contact);

    /// <summary>The first step in which they are no longer touching.</summary>
    /// <param name="contact">What was touched. The normal is the one from the last step.</param>
    void OnCollisionExit(Contact contact);

    /// <summary>The first step in which the body overlaps a trigger, or is one.</summary>
    /// <param name="contact">What was entered.</param>
    void OnTriggerEnter(Contact contact);

    /// <summary>Every step after the first in which the overlap continues.</summary>
    /// <param name="contact">What is being overlapped.</param>
    void OnTriggerStay(Contact contact);

    /// <summary>The first step in which the overlap has ended.</summary>
    /// <param name="contact">What was left.</param>
    void OnTriggerExit(Contact contact);
}
