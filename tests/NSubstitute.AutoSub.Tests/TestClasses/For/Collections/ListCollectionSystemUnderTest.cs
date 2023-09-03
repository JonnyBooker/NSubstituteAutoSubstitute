using NSubstitute.AutoSub.Tests.TestClasses.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;

namespace NSubstitute.AutoSub.Tests.TestClasses.Collections;

public class ListCollectionSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IList<IStringGenerationDependency> _stringGenerationDependencies;

    public ListCollectionSystemUnderTest(IList<IStringGenerationDependency> stringGenerationDependencies)
    {
        _stringGenerationDependencies = stringGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _stringGenerationDependencies.Select(x => x.Generate()));
    }
}