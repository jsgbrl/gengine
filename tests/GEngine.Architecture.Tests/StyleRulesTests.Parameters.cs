// Counting a parameter list without parsing one.
//
// Commas at the top level of the parentheses, ignoring the ones inside a generic argument -
// `Dictionary<string, int> map` is one parameter, not two.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

/// <content>The parameter budget.</content>
public sealed partial class StyleRulesTests
{
    private static void CheckParameters(SourceFile file, Violations violations)
    {
        foreach (Block block in Declarations.Blocks(file))
        {
            Count(file, block, violations);
        }
    }

    // An interop file transcribes somebody else's header. An IOKit callback takes seven
    // arguments whether we like it or not, and changing the signature means the callback is
    // never called.
    private static void Count(SourceFile file, Block block, Violations violations)
    {
        if (!block.IsMember || block.IsType || file.IsInterop)
        {
            return;
        }

        int parameters = ParametersIn(block.Signature);
        if (parameters > MostParameters)
        {
            violations.Add(file, block.FirstLine, block.Name + " takes " + parameters + " parameters");
        }
    }

    private static int ParametersIn(string header)
    {
        int open = header.IndexOf('(', StringComparison.Ordinal);
        int close = header.LastIndexOf(')');
        if (open < 0 || close < open)
        {
            return 0;
        }

        string list = header[(open + 1)..close];
        return list.Trim().Length == 0 ? 0 : TopLevelCommas(list) + 1;
    }

    private static int TopLevelCommas(string list)
    {
        var depth = new Depths();
        int commas = 0;
        foreach (char character in list)
        {
            depth.Read(character);
            commas += character == ',' && depth.IsTopLevel ? 1 : 0;
        }

        return commas;
    }

    private sealed class Depths
    {
        private static readonly Dictionary<char, int> Steps = new()
        {
            ['<'] = 1,
            ['>'] = -1,
            ['('] = 1,
            [')'] = -1,
            ['['] = 1,
            [']'] = -1,
        };

        private int _depth;

        public bool IsTopLevel => _depth == 0;

        public void Read(char character) => _depth += Steps.TryGetValue(character, out int step) ? step : 0;
    }
}
