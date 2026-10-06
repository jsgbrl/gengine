// Blanks out everything that is not code: comments, string literals and character literals.
//
// Without this every rule here has the same bug. A file that documents the `#region` rule
// contains the word `#region`; a test for the `this.` rule contains `this.`. Judging raw text
// makes the linter fail on its own documentation, so what the rules read is code with the
// prose replaced by spaces - same line numbers, same columns, no false accusations.

using System.Text;

namespace GEngine.Architecture.Tests;

internal static class CodeScanner
{
    /// <summary>Replaces comments and literals with spaces, keeping every line and column.</summary>
    /// <param name="text">The file's contents.</param>
    /// <returns>The same text with the prose blanked.</returns>
    public static string StripProse(string text)
    {
        var code = new StringBuilder(text.Length);
        var state = new ScanState();
        for (int index = 0; index < text.Length; index++)
        {
            int before = index;
            code.Append(Next(text, ref index, state));

            // Whatever the scanner swallowed - the `/` of a `*/`, the second half of an escape -
            // still has to occupy a column, or every line number after it slides.
            code.Append(' ', index - before);
        }

        return code.ToString();
    }

    private static char Next(string text, ref int index, ScanState state)
    {
        char character = text[index];
        if (character is '\n' or '\r')
        {
            state.EndLine();
            return character;
        }

        return state.IsInside ? Inside(text, ref index, state) : Outside(text, ref index, state);
    }

    // Entering a comment or a literal: everything from here to its end becomes spaces.
    private static char Outside(string text, ref int index, ScanState state)
    {
        char character = text[index];
        char following = index + 1 < text.Length ? text[index + 1] : '\0';
        if (character == '/' && following is '/' or '*')
        {
            state.Enter(following == '/' ? Region.LineComment : Region.BlockComment);
            return ' ';
        }

        return character == '"' || character == '\'' ? Quote(text, ref index, state) : character;
    }

    // Three kinds of quote open three different things, and the raw one has to be tried first:
    // the first character of a `"""` is also a perfectly good ordinary quote.
    private static char Quote(string text, ref int index, ScanState state)
    {
        int run = Run(text, index);
        if (run >= 3)
        {
            state.EnterRaw(run);
            index += run - 1;
            return ' ';
        }

        state.Enter(text[index] == '\'' ? Region.Character : Region.Text);
        state.IsVerbatim = index > 0 && text[index - 1] == '@';
        return ' ';
    }

    private static char Inside(string text, ref int index, ScanState state)
    {
        if (state.Region == Region.BlockComment && text[index] == '*' && Peek(text, index) == '/')
        {
            index++;
            state.Leave();
            return ' ';
        }

        if (state.Region == Region.Raw)
        {
            LeaveRaw(text, ref index, state);
            return ' ';
        }

        if (state.Region is Region.Text or Region.Character)
        {
            Escape(text, ref index, state);
        }

        return ' ';
    }

    private static void Escape(string text, ref int index, ScanState state)
    {
        char quote = state.Region == Region.Text ? '"' : '\'';
        if (!state.IsVerbatim && text[index] == '\\' && index + 1 < text.Length)
        {
            index++;
            return;
        }

        if (text[index] != quote)
        {
            return;
        }

        // In a verbatim string a doubled quote is one quote, not the end of the string.
        if (state.IsVerbatim && Peek(text, index) == '"')
        {
            index++;
            return;
        }

        state.Leave();
    }

    // A raw string ends at a run of quotes at least as long as the one that opened it, and
    // nothing else ends it - not a newline, not a backslash, not a doubled quote.
    private static void LeaveRaw(string text, ref int index, ScanState state)
    {
        int run = text[index] == '"' ? Run(text, index) : 0;
        if (run < state.Quotes)
        {
            return;
        }

        index += run - 1;
        state.Leave();
    }

    private static int Run(string text, int index)
    {
        int length = 0;
        while (index + length < text.Length && text[index + length] == '"')
        {
            length++;
        }

        return length;
    }

    private static char Peek(string text, int index) => index + 1 < text.Length ? text[index + 1] : '\0';

    private enum Region
    {
        None,
        LineComment,
        BlockComment,
        Text,
        Character,
        Raw,
    }

    private sealed class ScanState
    {
        public Region Region { get; private set; } = Region.None;

        public bool IsVerbatim { get; set; }

        public bool IsInside => Region != Region.None;

        public int Quotes { get; private set; }

        public void Enter(Region region) => Region = region;

        public void EnterRaw(int quotes)
        {
            Region = Region.Raw;
            Quotes = quotes;
        }

        public void Leave()
        {
            Region = Region.None;
            IsVerbatim = false;
            Quotes = 0;
        }

        // A line comment ends at the newline; so does a non-verbatim string, because an
        // unterminated one is a compiler error and never reaches this linter.
        public void EndLine()
        {
            if (Region is Region.LineComment or Region.Character || (Region == Region.Text && !IsVerbatim))
            {
                Leave();
            }
        }
    }
}
