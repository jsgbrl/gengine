// What the loop calls. Three methods, three different clocks, and the split between them
// is the single most important idea in the engine - docs/architecture.md explains why.

namespace GEngine.Core.Loop;

/// <summary>
/// The three callbacks a <see cref="GameLoop"/> drives. Anything that reacts to input or
/// animates goes in <see cref="Update"/>; anything that has to be reproducible - physics,
/// gameplay rules - goes in <see cref="FixedUpdate"/>; drawing goes in
/// <see cref="Render"/> and changes no state at all.
/// </summary>
public interface IGame
{
    /// <summary>Called once per frame with the real time that frame took.</summary>
    /// <param name="deltaSeconds">Seconds since the previous frame, already clamped.</param>
    void Update(float deltaSeconds);

    /// <summary>
    /// Called zero or more times per frame, always with the same delta. Two runs fed the
    /// same input see the same sequence of calls, which is what makes a replay possible.
    /// </summary>
    /// <param name="fixedDeltaSeconds">The fixed step, in seconds. Always the same value.</param>
    void FixedUpdate(float fixedDeltaSeconds);

    /// <summary>Called once per frame, after the fixed steps.</summary>
    /// <param name="interpolation">
    /// How far the simulation is between the last fixed step and the next one, from zero
    /// to just under one. Drawing at the last state ignores it and stutters; drawing at
    /// the interpolated state is what makes 60 fixed steps look smooth at 144 frames.
    /// </param>
    void Render(float interpolation);
}
