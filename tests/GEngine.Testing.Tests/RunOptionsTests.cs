using System;

namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="RunOptions"/>.</summary>
public sealed class RunOptionsTests
{
    [Test]
    public void Parse_WithNoArguments_RunsEverythingQuietly()
    {
        var options = RunOptions.Parse([]);
        Assert.AreEqual(string.Empty, options.Filter);
        Assert.IsFalse(options.ListOnly);
        Assert.IsFalse(options.Verbose);
    }

    [Test]
    public void Parse_ReadsTheFilterText()
    {
        Assert.AreEqual("Physics", RunOptions.Parse(["--filter=Physics"]).Filter);
    }

    [Test]
    public void Parse_ReadsTheFlagsInAnyOrder()
    {
        var options = RunOptions.Parse(["--verbose", "--list"]);
        Assert.IsTrue(options.ListOnly);
        Assert.IsTrue(options.Verbose);
    }

    [Test]
    public void Parse_RejectsAnUnknownArgument_SoATypoNeverRunsNothing()
    {
        ArgumentException failure = Assert.Throws<ArgumentException>(static () => RunOptions.Parse(["--filtr=x"]));
        Assert.IsTrue(failure.Message.Contains("--filtr=x", StringComparison.Ordinal));
    }

    [Test]
    public void Usage_NamesEveryAcceptedArgument()
    {
        string usage = RunOptions.Usage();
        Assert.IsTrue(usage.Contains("--filter=", StringComparison.Ordinal));
        Assert.IsTrue(usage.Contains("--list", StringComparison.Ordinal));
        Assert.IsTrue(usage.Contains("--verbose", StringComparison.Ordinal));
    }
}
