// Reports as hexadecimal text, so a captured report can live in a file, be pasted into a bug
// report, and be diffed. This is what turns "my triangle button does not work" into a line
// somebody else can reproduce.

using System;
using System.Globalization;
using System.Text;

namespace GEngine.Input.Hid;

/// <summary>Converts HID reports between bytes and hexadecimal text.</summary>
public static class HidReportHex
{
    /// <summary>Formats a report as space-separated two-digit hexadecimal.</summary>
    /// <param name="report">The bytes.</param>
    /// <returns>Text such as "01 80 80 7F".</returns>
    public static string ToText(ReadOnlySpan<byte> report)
    {
        var text = new StringBuilder(report.Length * 3);
        for (int index = 0; index < report.Length; index++)
        {
            if (index > 0)
            {
                text.Append(' ');
            }

            text.Append(report[index].ToString("X2", CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }

    /// <summary>Reads a report from hexadecimal text, ignoring whitespace and comments.</summary>
    /// <param name="text">The text. A hash starts a comment.</param>
    /// <returns>The bytes.</returns>
    /// <exception cref="FormatException">The text is not pairs of hexadecimal digits.</exception>
    public static byte[] Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        string[] tokens = Strip(text).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        byte[] report = new byte[tokens.Length];
        for (int index = 0; index < tokens.Length; index++)
        {
            if (!byte.TryParse(tokens[index], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out report[index]))
            {
                throw new FormatException("not a pair of hexadecimal digits: " + tokens[index]);
            }
        }

        return report;
    }

    private static string Strip(string text)
    {
        var stripped = new StringBuilder(text.Length);
        foreach (string raw in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            int comment = raw.IndexOf('#', StringComparison.Ordinal);
            stripped.Append(comment < 0 ? raw : raw[..comment]).Append(' ');
        }

        return stripped.ToString();
    }
}
