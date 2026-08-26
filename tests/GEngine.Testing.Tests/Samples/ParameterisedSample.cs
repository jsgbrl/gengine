// Three cases from one method, plus a case whose argument needs converting from the int
// the attribute stores to the float the method declares.

namespace GEngine.Testing.Tests.Samples;

internal sealed class ParameterisedSample
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void EachValueIsPositive(int value)
    {
        Assert.IsTrue(value > 0);
    }

    [TestCase(2, 1.5f)]
    public void ConvertsArgumentsToTheDeclaredTypes(float whole, float fraction)
    {
        Assert.ApproximatelyEqual(3.5f, whole + fraction);
    }
}
