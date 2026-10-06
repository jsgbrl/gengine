// Finding the blocks in a file without parsing C#.
//
// The trick is that a block's header is everything since the last `;`, `{` or `}`. For a method
// that is its signature; for an `if` it is the condition; for a type it is the declaration.
// That one rule is enough to measure every budget in docs/style.md, and it is about a hundred
// lines rather than a compiler.

using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

internal static class Declarations
{
    /// <summary>Every braced block in a file, outermost first.</summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The blocks.</returns>
    public static List<Block> Blocks(SourceFile file)
    {
        var scan = new BlockScan();
        string code = string.Join("\n", file.CodeLines);
        for (int index = 0; index < code.Length; index++)
        {
            scan.Read(code, index);
        }

        return scan.Finished;
    }

    /// <summary>Every type declaration in a file.</summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The type blocks.</returns>
    public static List<Block> Types(SourceFile file) => Where(Blocks(file), wantTypes: true);

    /// <summary>Every method, constructor and accessor with a body.</summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The method blocks.</returns>
    public static List<Block> Methods(SourceFile file) => Where(Blocks(file), wantTypes: false);

    private static List<Block> Where(List<Block> blocks, bool wantTypes)
    {
        List<Block> wanted = [];
        foreach (Block block in blocks)
        {
            if (block.IsType == wantTypes && (wantTypes || block.IsMethod))
            {
                wanted.Add(block);
            }
        }

        return wanted;
    }

    // One pass over the file, keeping a stack of open blocks and the line each one started on.
    // A header starts after the previous `;`, `{` or `}` and is measured from its first real
    // character, so a signature spread over three lines still reports the line a reader sees.
    private sealed class BlockScan
    {
        private readonly Stack<int> _openLines = new();
        private readonly Stack<int> _openHeaders = new();
        private int _line = 1;
        private int _headerStart;
        private int _headerLine;

        public List<Block> Finished { get; } = [];

        public void Read(string code, int index)
        {
            char character = code[index];
            if (character == '\n')
            {
                _line++;
                return;
            }

            if (_headerLine == 0 && !char.IsWhiteSpace(character))
            {
                _headerLine = _line;
            }

            Punctuation(code, index, character);
        }

        private void Punctuation(string code, int index, char character)
        {
            if (character == ';')
            {
                Restart(index);
                return;
            }

            if (character == '{')
            {
                Open(index);
                return;
            }

            if (character == '}')
            {
                Close(code, index);
            }
        }

        private void Restart(int index)
        {
            _headerStart = index + 1;
            _headerLine = 0;
        }

        private void Open(int index)
        {
            _openHeaders.Push(_headerStart);
            _openLines.Push(_headerLine == 0 ? _line : _headerLine);
            Restart(index);
        }

        private void Close(string code, int index)
        {
            if (_openLines.Count == 0)
            {
                Restart(index);
                return;
            }

            int headerStart = _openHeaders.Pop();
            int firstLine = _openLines.Pop();
            string header = code[headerStart..FindBrace(code, headerStart, index)];
            Finished.Add(new Block(header, firstLine, _line, _openLines.Count));
            Restart(index);
        }

        // The header ends at the brace that opened this block: the first `{` after it started.
        private static int FindBrace(string code, int from, int to)
        {
            for (int index = from; index < to; index++)
            {
                if (code[index] == '{')
                {
                    return index;
                }
            }

            return to;
        }
    }
}
