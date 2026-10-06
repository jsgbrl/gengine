using System;
using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Audio;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The backend that remembers instead of making a noise. It turns out to be the most useful
/// object in the suite: it is the game narrating itself, so a test can ask "did that stomp?"
/// without looking at pixels.
/// </summary>
public sealed class MemoryAudioBackendTests
{
    [Test]
    public void TheRememberingBackendRemembersInOrder()
    {
        var audio = new MemoryAudioBackend();
        audio.Play(GameSound.Jump);
        audio.Play(GameSound.Coin);
        audio.Play(GameSound.Jump);

        Assert.AreEqual(3, audio.Played.Count);
        Assert.AreEqual(GameSound.Jump, audio.Played[0]);
        Assert.AreEqual(GameSound.Coin, audio.Played[1]);
        Assert.AreEqual(2, audio.CountOf(GameSound.Jump));
        Assert.AreEqual(0, audio.CountOf(GameSound.Stomp), "nothing was stomped");
    }

    [Test]
    public void ClearingForgetsEverything()
    {
        var audio = new MemoryAudioBackend();
        audio.Play(GameSound.Coin);
        audio.Clear();
        Assert.AreEqual(0, audio.Played.Count);
        Assert.AreEqual(0, audio.CountOf(GameSound.Coin));
    }

    [Test]
    public void JumpingMakesTheJumpSound()
    {
        var world = new TestWorld("tiles:\n.M.\n###\n");
        world.Step(10);
        world.Audio.Clear();
        world.HoldFor(InputAction.Jump, 5);

        Assert.AreEqual(1, world.Audio.CountOf(GameSound.Jump), "one press, one sound");
    }

    [Test]
    public void ACoinMakesTheCoinSoundAndNothingElse()
    {
        var world = new TestWorld("tiles:\n.M..o.\n######\n");
        world.Hold(InputAction.MoveRight);
        world.Step(60);

        Assert.AreEqual(1, world.Audio.CountOf(GameSound.Coin));
        Assert.AreEqual(0, world.Audio.CountOf(GameSound.Stomp));
    }

    [Test]
    public void EverySoundTheGameCanMakeIsNamed()
    {
        foreach (GameSound sound in Enum.GetValues<GameSound>())
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(sound.ToString()), "an unnamed sound is an unusable one");
        }
    }
}
