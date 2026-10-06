using System;
using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// One concept, one word. The table lives in docs/glossary.md and this reads it, so the
/// documentation and the rule cannot drift apart - adding a row to the table starts enforcing
/// it, and removing a word from the code makes the table's own test fail.
///
/// A synonym is banned in the project that owns the word, not everywhere: `surface` is wrong in
/// Rendering, where the word is frame buffer, and right in Physics, where a surface is the thing
/// you land on.
/// </summary>
public sealed partial class VocabularyTests
{
    private static readonly Dictionary<string, string[]> BannedIn = new(StringComparer.Ordinal)
    {
        ["GEngine.Rendering"] = ["canvas", "texture", "bitmap", "surface", "blit", "swap", "viewport", "paint", "plot"],
        ["GEngine.Physics"] = ["collider", "intersection"],
        ["GEngine.Input"] = ["packet", "intent"],
        ["GEngine.Core"] = ["timestep", "game object", "dt"],
        ["MarioClone"] = ["game object", "sprite sheet"],
    };

    [Test]
    public void NoProjectUsesASynonymForAWordItOwns()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckSynonyms(file, violations);
        }

        violations.AssertNone("a project never uses a synonym for a word it owns - see docs/glossary.md");
    }

    [Test]
    public void NothingIsAbbreviated()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckAbbreviations(file, violations);
        }

        violations.AssertNone("abbreviations are spelled out, except " + string.Join(", ", Vocabulary.AllowedAbbreviations));
    }

    // A word nobody uses is a word the glossary invented. The table is a description of the
    // code, and a description that has stopped being true is worse than no description.
    [Test]
    public void EveryWordInTheGlossaryIsUsedSomewhere()
    {
        var violations = new Violations();
        foreach (string word in Vocabulary.Words)
        {
            CheckUsed(word, violations);
        }

        violations.AssertNone("every word in docs/glossary.md is a word the code actually uses");
    }

    [Test]
    public void EveryBannedSynonymComesFromTheGlossary()
    {
        var violations = new Violations();
        foreach (KeyValuePair<string, string[]> project in BannedIn)
        {
            CheckDeclared(project, violations);
        }

        violations.AssertNone("every synonym this test bans is one docs/glossary.md bans");
    }

    [Test]
    public void TheVocabularyRulesGoRedWhenTheyShould()
    {
        var synonym = new Violations();
        CheckSynonyms(new SourceFile("src/GEngine.Rendering/Fake.cs", "class Fake { Texture _t; }"), synonym);
        Assert.Throws<AssertionException>(() => synonym.AssertNone("synonyms"), "Texture in Rendering went unnoticed");

        var allowed = new Violations();
        CheckSynonyms(new SourceFile("src/GEngine.Physics/Fake.cs", "class Fake { float Surface; }"), allowed);
        allowed.AssertNone("a surface in the physics world is a surface");

        var short_ = new Violations();
        CheckAbbreviations(new SourceFile("src/Fake.cs", "class Fake { int _idx; }"), short_);
        Assert.Throws<AssertionException>(() => short_.AssertNone("abbreviations"), "_idx went unnoticed");
    }

    private static void CheckDeclared(KeyValuePair<string, string[]> project, Violations violations)
    {
        foreach (string synonym in project.Value)
        {
            if (!Vocabulary.Forbids(synonym))
            {
                violations.Add("docs/glossary.md", "nothing in the table forbids '" + synonym + "'");
            }
        }
    }

    private static void CheckUsed(string word, Violations violations)
    {
        string wanted = word.Replace(" ", string.Empty, StringComparison.Ordinal);
        foreach (SourceFile file in Repository.Sources)
        {
            if (file.Text.Contains(wanted, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        violations.Add("docs/glossary.md", "nothing in the repository says '" + word + "'");
    }
}
