namespace NSubstitute.AutoSub.Tests.PartsOf.Systems;

public class SimplePartsOfSystemUnderTest
{
    private readonly SimplePartsOfPartsOfDependency _simplePartsOfPartsOfDependency;

    public SimplePartsOfSystemUnderTest(SimplePartsOfPartsOfDependency simplePartsOfPartsOfDependency)
    {
        _simplePartsOfPartsOfDependency = simplePartsOfPartsOfDependency;
    }

    public string Invoke()
    {
        return _simplePartsOfPartsOfDependency.PartsOfInvoke();
    }
}