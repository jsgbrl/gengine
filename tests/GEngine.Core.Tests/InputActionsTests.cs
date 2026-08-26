using System;
using GEngine.Core.Contracts;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>
/// Covers <see cref="InputActions"/>. The hand-written list exists so that nothing else has
/// to use reflection; this test uses it once, here, to prove the list is complete - which is
/// the one place where reflection buys something the hand-written list cannot.
/// </summary>
public sealed class InputActionsTests
{
    [Test]
    public void TheListHoldsEveryActionOfTheEnum()
    {
        InputAction[] declared = Enum.GetValues<InputAction>();
        Assert.AreEqual(declared.Length, InputActions.All.Count);
        foreach (InputAction action in declared)
        {
            Assert.IsTrue(Holds(action), action + " is missing from InputActions.All");
        }
    }

    [Test]
    public void TheCountMatchesTheList()
    {
        Assert.AreEqual(InputActions.Count, InputActions.All.Count);
    }

    [Test]
    public void TheListIsInDeclarationOrder_SoAnIndexIsTheEnumValue()
    {
        for (int index = 0; index < InputActions.All.Count; index++)
        {
            Assert.AreEqual(index, (int)InputActions.All[index]);
        }
    }

    private static bool Holds(InputAction wanted)
    {
        foreach (InputAction action in InputActions.All)
        {
            if (action == wanted)
            {
                return true;
            }
        }

        return false;
    }
}
