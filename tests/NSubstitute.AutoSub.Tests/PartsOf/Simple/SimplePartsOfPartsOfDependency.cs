namespace NSubstitute.AutoSub.Tests.PartsOf.Simple;

public class SimplePartsOfPartsOfDependency
{
    public const string NonMockedText = "NonMockedText";
    public const string MockedText = "MockedText";
    
    public string NonMockedMethod()
    {
        return NonMockedText;
    }

    public virtual string MockedMethod()
    {
        return MockedText;
    }
}