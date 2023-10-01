using NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections;

public class EnumerablePartsOfSystemUnderTest : ICollectionPartsOfSystemUnderTest
{
    private readonly IEnumerable<SimplePartsOfPartsOfDependency> _simplePartsOfPartsOfDependencies;

    public EnumerablePartsOfSystemUnderTest(IEnumerable<SimplePartsOfPartsOfDependency> simplePartsOfPartsOfDependencies)
    {
        _simplePartsOfPartsOfDependencies = simplePartsOfPartsOfDependencies;
    }
    
    public string Generate()
    {
        return string.Join(" ", _simplePartsOfPartsOfDependencies.Select(x => x.PartsOfInvoke()));
    }
}