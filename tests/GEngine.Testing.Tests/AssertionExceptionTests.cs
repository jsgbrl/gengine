using System;

namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="AssertionException"/>.</summary>
public sealed class AssertionExceptionTests
{
    [Test]
    public void Constructors_CarryMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        Assert.AreEqual("why", new AssertionException("why").Message);
        Assert.AreSame(inner, new AssertionException("why", inner).InnerException);
        Assert.IsNotNull(new AssertionException().Message);
    }
}
