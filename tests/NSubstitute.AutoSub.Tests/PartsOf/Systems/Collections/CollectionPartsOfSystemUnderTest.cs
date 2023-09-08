using NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections;

public class CollectionPartsOfSystemUnderTest : ICollectionPartsOfSystemUnderTest
{
    private readonly ICollection<SimplePartsOfPartsOfDependency> _simplePartsOfPartsOfDependencies;

    public CollectionPartsOfSystemUnderTest(ICollection<SimplePartsOfPartsOfDependency> simplePartsOfPartsOfDependencies)
    {
        _simplePartsOfPartsOfDependencies = simplePartsOfPartsOfDependencies;
    }
    
    public string Generate()
    {
        return string.Join(" ", _simplePartsOfPartsOfDependencies.Select(x => x.MockedMethod()));
    }
}