namespace NSubstitute.AutoSub.Tests.PartsOf.Systems;

public class SimplePartsOfPartsOfDependency
{
    public const string NonMockedText = "NonMockedText";
    
    public virtual string PartsOfInvoke()
    {
        return NonMockedText;
    }
}