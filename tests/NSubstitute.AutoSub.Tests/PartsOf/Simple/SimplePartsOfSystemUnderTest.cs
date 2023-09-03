namespace NSubstitute.AutoSub.Tests.PartsOf.Simple;

public class SimplePartsOfSystemUnderTest
{
    private readonly SimplePartsOfPartsOfDependency _simplePartsOfPartsOfDependency;

    public SimplePartsOfSystemUnderTest(SimplePartsOfPartsOfDependency simplePartsOfPartsOfDependency)
    {
        _simplePartsOfPartsOfDependency = simplePartsOfPartsOfDependency;
    }

    public string InvokeMocked()
    {
        return _simplePartsOfPartsOfDependency.MockedMethod();
    }

    public string InvokeNotMocked()
    {
        return _simplePartsOfPartsOfDependency.NonMockedMethod();
    }
}