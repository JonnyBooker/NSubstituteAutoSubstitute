using NSubstitute.AutoSub.Tests.PartsOf.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.PartsOf.Simple;

namespace NSubstitute.AutoSub.Tests.PartsOf.Collections;

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