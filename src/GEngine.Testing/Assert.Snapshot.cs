// Snapshot assertion. A rendering test produces a block of text and compares it to the
// block written into the test; on failure the report points at the first line that
// differs instead of dumping two screens and letting you find it.

using System;
using System.Globalization;

namespace GEngine.Testing;

/// <content>Assertion for multi-line text captures.</content>
public static partial class Assert
{
    /// <summary>Fails unless a multi-line capture matches the expected text.</summary>
    /// <param name="actual">The capture the code produced.</param>
    /// <param name="expected">The capture the test expects.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void MatchesSnapshot(string actual, string expected, string message = "")
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expected);

        string[] actualLines = SplitLines(actual);
        string[] expectedLines = SplitLines(expected);
        int firstDifference = FirstDifferentLine(expectedLines, actualLines);
        if (firstDifference < 0)
        {
            return;
        }

        throw new AssertionException(Describe(
            LineAt(expectedLines, firstDifference),
            LineAt(actualLines, firstDifference),
            Headline(message, firstDifference)));
    }

    private static string Headline(string message, int lineIndex)
    {
        string where = string.Format(
            CultureInfo.InvariantCulture,
            "snapshot differs at line {0}",
            lineIndex + 1);

        return message.Length == 0 ? where : message + Environment.NewLine + where;
    }

    private static int FirstDifferentLine(string[] expected, string[] actual)
    {
        int shared = Math.Min(expected.Length, actual.Length);
        for (int index = 0; index < shared; index++)
        {
            if (!string.Equals(expected[index], actual[index], StringComparison.Ordinal))
            {
                return index;
            }
        }

        return expected.Length == actual.Length ? -1 : shared;
    }

    private static string LineAt(string[] lines, int index)
    {
        if (index >= lines.Length)
        {
            return "<end of capture>";
        }

        return "|" + lines[index] + "|";
    }

    private static string[] SplitLines(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
    }
}
