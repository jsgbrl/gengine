// An input source a test can turn on and off, and unplug mid-frame.

using GEngine.Core.Contracts;

namespace GEngine.Input.Tests.Doubles;

internal sealed class SwitchableBackend : IInputBackend
{
    private readonly float[] _values = new float[InputActions.Count];

    public SwitchableBackend(string name) => Name = name;

    public string Name { get; }

    public bool IsConnected { get; set; } = true;

    public int PollCount { get; private set; }

    public int DisposeCount { get; private set; }

    public void Set(InputAction action, float value) => _values[(int)action] = value;

    public void Poll(float deltaSeconds) => PollCount++;

    public bool IsDown(InputAction action) => _values[(int)action] >= 0.5f;

    public float AxisValue(InputAction action) => _values[(int)action];

    public void Dispose() => DisposeCount++;
}
