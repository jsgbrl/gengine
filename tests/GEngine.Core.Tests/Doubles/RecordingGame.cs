// A game that only writes down what the loop asked of it. Everything the loop promises -
// how many fixed steps, with what delta, in what order - is checked against this.

using System.Collections.Generic;
using GEngine.Core.Loop;

namespace GEngine.Core.Tests.Doubles;

internal sealed class RecordingGame : IGame
{
    private readonly List<string> _calls = [];

    public IReadOnlyList<string> Calls => _calls;

    public int UpdateCount { get; private set; }

    public int FixedUpdateCount { get; private set; }

    public int RenderCount { get; private set; }

    public float LastDeltaSeconds { get; private set; }

    public float LastFixedDeltaSeconds { get; private set; }

    public float LastInterpolation { get; private set; }

    public GameLoop? StopAfterFirstRender { get; set; }

    public void Update(float deltaSeconds)
    {
        UpdateCount++;
        LastDeltaSeconds = deltaSeconds;
        _calls.Add("update");
    }

    public void FixedUpdate(float fixedDeltaSeconds)
    {
        FixedUpdateCount++;
        LastFixedDeltaSeconds = fixedDeltaSeconds;
        _calls.Add("fixed");
    }

    public void Render(float interpolation)
    {
        RenderCount++;
        LastInterpolation = interpolation;
        _calls.Add("render");
        StopAfterFirstRender?.Stop();
    }
}
