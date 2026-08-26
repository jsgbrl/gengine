// What is inside a question block. Two options is all the original needs, and a third would be
// one line here and one case in QuestionBlock.Give.

namespace MarioClone.Actors;

/// <summary>What comes out of a question block.</summary>
public enum BlockPrize
{
    /// <summary>A coin, collected immediately.</summary>
    Coin,

    /// <summary>A mushroom, which walks out of the top of the block.</summary>
    Mushroom,
}
