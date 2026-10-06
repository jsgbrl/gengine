using GEngine.Testing;
using MarioClone.Audio;

namespace MarioClone.Tests;

/// <summary>
/// There is no sound. A console has no audio device the engine could portably reach, so what
/// the game has instead is the seam where sound would go: an interface, a backend that does
/// nothing, and a backend that remembers. The remembering one is how a test asks what happened
/// without looking at pixels - it is the game's own narration of itself.
/// </summary>
public sealed class NullAudioBackendTests
{
    [Test]
    public void TheSilentBackendIsSilentAndIsAlwaysThere()
    {
        Assert.IsNotNull(NullAudioBackend.Instance);
        Assert.AreSame(NullAudioBackend.Instance, NullAudioBackend.Instance, "one silence is enough");
        Assert.AreEqual("silent", NullAudioBackend.Instance.Name);
        NullAudioBackend.Instance.Play(GameSound.Jump);
    }
}
