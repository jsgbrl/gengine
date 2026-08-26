// A terminal that is a string. Everything the console renderer sends can be read back and
// asserted on, the size can be changed to simulate a resize, and no real terminal is
// touched - which matters because the test report is being printed on the real one.

using System.IO;
using System.Text;

namespace GEngine.Rendering.Tests.Doubles;

internal sealed class FakeConsoleDriver : IConsoleDriver
{
    private readonly StringWriter _output = new();

    public string Name => "fake terminal";

    public ColorDepth Depth { get; set; } = ColorDepth.TrueColor;

    public int Columns { get; set; } = 8;

    public int Rows { get; set; } = 4;

    public TextWriter Output => _output;

    public int EnableCount { get; private set; }

    public int RestoreCount { get; private set; }

    public string Written => _output.GetStringBuilder().ToString();

    public void Enable() => EnableCount++;

    public void Restore() => RestoreCount++;

    public void Dispose()
    {
        Restore();
        _output.Dispose();
    }

    public void ClearWritten() => _output.GetStringBuilder().Clear();

    public StringBuilder Builder => _output.GetStringBuilder();
}
