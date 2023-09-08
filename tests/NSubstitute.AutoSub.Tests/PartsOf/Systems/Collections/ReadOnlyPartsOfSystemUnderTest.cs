using NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections;

public class ReadOnlyPartsOfSystemUnderTest : ICollectionPartsOfSystemUnderTest
{
    private readonly IReadOnlyCollection<SimplePartsOfPartsOfDependency> _simplePartsOfPartsOfDependencies;

    public ReadOnlyPartsOfSystemUnderTest(IReadOnlyCollection<SimplePartsOfPartsOfDependency> simplePartsOfPartsOfDependencies)
    {
        _simplePartsOfPartsOfDependencies = simplePartsOfPartsOfDependencies;
    }
    
    public string Generate()
    {
        return string.Join(" ", _simplePartsOfPartsOfDependencies.Select(x => x.MockedMethod()));
    }
}