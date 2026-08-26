using System;
using GEngine.Input.Hid;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="HidReportHex"/>.</summary>
public sealed class HidReportHexTests
{
    [Test]
    public void BytesBecomeSpacedPairsOfDigits()
    {
        Assert.AreEqual("01 80 FF", HidReportHex.ToText([0x01, 0x80, 0xFF]));
        Assert.AreEqual(string.Empty, HidReportHex.ToText([]));
    }

    [Test]
    public void TextBecomesBytesAgain()
    {
        byte[] report = HidReportHex.Parse("01 80 FF");
        Assert.AreEqual(3, report.Length);
        Assert.AreEqual(0xFF, report[2]);
    }

    [Test]
    public void WhitespaceCommentsAndLineBreaksAreIgnored()
    {
        byte[] report = HidReportHex.Parse("# a comment\n01 80\n  FF   # trailing\n");
        Assert.AreEqual(3, report.Length);
    }

    [Test]
    public void RoundTrippingAFixtureGivesTheSameText()
    {
        byte[] report = Fixtures.Report("neutral.txt");
        Assert.AreEqual(report.Length, HidReportHex.Parse(HidReportHex.ToText(report)).Length);
    }

    [Test]
    public void SomethingThatIsNotHexadecimalIsAnError()
    {
        Assert.Throws<FormatException>(static () => HidReportHex.Parse("01 ZZ"));
    }
}
