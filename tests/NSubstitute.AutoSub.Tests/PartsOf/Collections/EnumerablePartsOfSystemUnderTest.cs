using NSubstitute.AutoSub.Tests.PartsOf.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.PartsOf.Simple;

namespace NSubstitute.AutoSub.Tests.PartsOf.Collections;

public class EnumerablePartsOfSystemUnderTest : ICollectionPartsOfSystemUnderTest
{
    private readonly IEnumerable<SimplePartsOfPartsOfDependency> _simplePartsOfPartsOfDependencies;

    public EnumerablePartsOfSystemUnderTest(IEnumerable<SimplePartsOfPartsOfDependency> simplePartsOfPartsOfDependencies)
    {
        _simplePartsOfPartsOfDependencies = simplePartsOfPartsOfDependencies;
    }
    
    public string Generate()
    {
        return string.Join(" ", _simplePartsOfPartsOfDependencies.Select(x => x.MockedMethod()));
    }
}