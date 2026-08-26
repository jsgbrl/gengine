// Where sound would go. There is no implementation that makes a noise, and that is a decision
// rather than an omission: the only thing in the base class library that beeps is
// Console.Beep, it exists on Windows and nowhere else, and rule 5 of the build prompt says the
// three systems are peers. A game that beeps on one of them and is silent on the other two is
// not a portable game, it is a Windows game with a portability claim.
//
// So the interface is here, NullAudioBackend is what the game uses, and the day a real one
// arrives it is one class and one line in the composition root.

namespace MarioClone.Audio;

/// <summary>Somewhere a sound can be played.</summary>
public interface IAudioBackend
{
    /// <summary>Human-readable name, shown in diagnostics.</summary>
    string Name { get; }

    /// <summary>Plays a sound, or does not.</summary>
    /// <param name="sound">Which sound.</param>
    void Play(GameSound sound);
}
