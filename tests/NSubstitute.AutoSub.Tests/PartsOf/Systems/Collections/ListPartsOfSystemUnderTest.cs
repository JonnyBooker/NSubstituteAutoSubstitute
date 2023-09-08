using NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections;

public class ListPartsOfSystemUnderTest : ICollectionPartsOfSystemUnderTest
{
    private readonly IList<SimplePartsOfPartsOfDependency> _simplePartsOfPartsOfDependencies;

    public ListPartsOfSystemUnderTest(IList<SimplePartsOfPartsOfDependency> simplePartsOfPartsOfDependencies)
    {
        _simplePartsOfPartsOfDependencies = simplePartsOfPartsOfDependencies;
    }
    
    public string Generate()
    {
        return string.Join(" ", _simplePartsOfPartsOfDependencies.Select(x => x.MockedMethod()));
    }
}