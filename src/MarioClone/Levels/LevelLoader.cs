// A level is a text file, and this is the thirty lines that read it.
//
// The grid begins after a line saying `tiles:`, exactly the way a sprite's picture begins after
// `pixels:`. That is not decoration: in a level `#` is the ground, so `#` cannot also start a
// comment - the first version of this loader silently threw away every row of floor in the
// game. Everything before `tiles:` is a header and is ignored; everything after it is a row.

using System;
using System.Collections.Generic;

namespace MarioClone.Levels;

/// <summary>Reads the level file format.</summary>
public static class LevelLoader
{
    /// <summary>The line that separates a level's header from its grid.</summary>
    public const string TilesMarker = "tiles:";

    /// <summary>Parses a level.</summary>
    /// <param name="text">The file contents.</param>
    /// <param name="tileSize">Width and height of one tile, in pixels.</param>
    /// <returns>The level.</returns>
    /// <exception cref="FormatException">The text is not a well-formed level.</exception>
    public static Level Parse(string text, float tileSize)
    {
        ArgumentNullException.ThrowIfNull(text);
        List<string> rows = ReadRows(text);
        if (rows.Count == 0)
        {
            throw new FormatException("a level needs at least one row");
        }

        var level = new Level(rows[0].Length, rows.Count, tileSize);
        for (int row = 0; row < rows.Count; row++)
        {
            ReadRow(level, rows[row], row);
        }

        return level;
    }

    private static void ReadRow(Level level, string row, int rowIndex)
    {
        if (row.Length != level.Columns)
        {
            throw new FormatException("row " + rowIndex + " is " + row.Length + " wide, not " + level.Columns);
        }

        for (int column = 0; column < row.Length; column++)
        {
            ReadCell(level, row[column], column, rowIndex);
        }
    }

    // A character is either a tile or a thing. A thing leaves the cell empty behind it: a coin
    // is not something you can stand on, and neither is a goomba.
    private static void ReadCell(Level level, char symbol, int column, int row)
    {
        if (LevelLegend.Tiles.TryGetValue(symbol, out TileKind tile))
        {
            level.SetTile(column, row, tile);
            return;
        }

        if (!LevelLegend.Spawns.TryGetValue(symbol, out SpawnKind spawn))
        {
            throw new FormatException("nothing in the legend is called '" + symbol + "'");
        }

        level.SetTile(column, row, TileKind.Empty);
        level.AddSpawn(new LevelSpawn(spawn, column, row));
        if (spawn == SpawnKind.Player)
        {
            level.PlayerStart = level.TileCenter(column, row);
        }
    }

    private static List<string> ReadRows(string text)
    {
        List<string> rows = [];
        bool inGrid = false;
        foreach (string raw in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            string row = raw.TrimEnd();
            if (!inGrid)
            {
                inGrid = row.Trim().Equals(TilesMarker, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (row.Length > 0)
            {
                rows.Add(row);
            }
        }

        if (!inGrid)
        {
            throw new FormatException("a level needs a line saying " + TilesMarker + " before its grid");
        }

        return rows;
    }
}
