using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>
/// The test that gives the Strategy pattern its point: the fast broad phase and the slow
/// one must return exactly the same pairs, in exactly the same order, for every layout.
/// An optimisation is allowed to change how long something takes and nothing else.
/// </summary>
public sealed class BroadPhaseEquivalenceTests
{
    [Test]
    public void OnAScatteredLayout_BothBroadPhasesAgree()
    {
        AssertAgreement(Scattered());
    }

    [Test]
    public void OnAPileWhereEverythingOverlaps_BothBroadPhasesAgree()
    {
        AssertAgreement(Pile());
    }

    [Test]
    public void OnBodiesStraddlingCellBoundaries_BothBroadPhasesAgree()
    {
        AssertAgreement(Straddling());
    }

    [Test]
    public void OnAnEmptyWorld_BothBroadPhasesAgree()
    {
        AssertAgreement(new BodySet());
    }

    [Test]
    public void TheGridMakesFarFewerComparisonsThanBruteForce()
    {
        BodySet bodies = Scattered();
        var brute = new BruteForceBroadPhase();
        var grid = new SpatialHashGrid(32.0f);
        List<BroadPhasePair> buffer = [];

        brute.Update(bodies.All);
        brute.FindPairs(buffer);
        grid.Update(bodies.All);
        grid.FindPairs(buffer);

        Assert.IsTrue(grid.LastComparisonCount < brute.LastComparisonCount / 4, "the grid is doing real work");
    }

    private static void AssertAgreement(BodySet bodies)
    {
        var brute = new BruteForceBroadPhase();
        var grid = new SpatialHashGrid(32.0f);
        List<BroadPhasePair> fromBrute = [];
        List<BroadPhasePair> fromGrid = [];

        brute.Update(bodies.All);
        brute.FindPairs(fromBrute);
        grid.Update(bodies.All);
        grid.FindPairs(fromGrid);

        Assert.MatchesSnapshot(Describe(fromGrid), Describe(fromBrute));
    }

    private static string Describe(List<BroadPhasePair> pairs)
    {
        List<string> lines = [];
        foreach (BroadPhasePair pair in pairs)
        {
            lines.Add(pair.ToString());
        }

        return string.Join("\n", lines);
    }

    // Forty bodies on a coarse lattice: most pairs are far apart, a few overlap.
    private static BodySet Scattered()
    {
        var bodies = new BodySet();
        for (int index = 0; index < 40; index++)
        {
            bodies.AddBoxAt(((index * 37) % 400) + (index % 3), ((index * 53) % 300) + (index % 5));
        }

        return bodies;
    }

    // Twelve bodies inside one another, so every pair is a pair.
    private static BodySet Pile()
    {
        var bodies = new BodySet();
        for (int index = 0; index < 12; index++)
        {
            bodies.AddBoxAt(100.0f + (index * 0.5f), 100.0f);
        }

        return bodies;
    }

    // Boxes sitting exactly on the grid seams, which is where a hash grid loses pairs if it
    // files a body into one cell instead of every cell it touches.
    private static BodySet Straddling()
    {
        var bodies = new BodySet();
        for (int index = 0; index < 10; index++)
        {
            bodies.Add(BodyType.Dynamic, new Vector2(32.0f * index, 32.0f), new Vector2(40.0f, 40.0f));
        }

        return bodies;
    }
}
