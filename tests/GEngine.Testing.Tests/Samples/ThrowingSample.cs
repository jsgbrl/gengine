// A sample that throws something other than AssertionException: the runner has to tell
// "the code under test is wrong" from "the test itself is wrong".

using System;

namespace GEngine.Testing.Tests.Samples;

internal sealed class ThrowingSample
{
    internal const string ExpectedMessage = "not an assertion";

    [Test]
    public void AlwaysThrows()
    {
        throw new InvalidOperationException(ExpectedMessage);
    }
}
