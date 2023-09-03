using NSubstitute.AutoSub.Tests.PartsOf.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.PartsOf.Simple;

namespace NSubstitute.AutoSub.Tests.PartsOf.Collections;

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