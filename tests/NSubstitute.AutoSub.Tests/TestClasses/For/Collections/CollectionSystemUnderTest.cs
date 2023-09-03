using NSubstitute.AutoSub.Tests.TestClasses.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;

namespace NSubstitute.AutoSub.Tests.TestClasses.Collections;

public class CollectionSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly ICollection<IStringGenerationDependency> _stringGenerationDependencies;

    public CollectionSystemUnderTest(ICollection<IStringGenerationDependency> stringGenerationDependencies)
    {
        _stringGenerationDependencies = stringGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _stringGenerationDependencies.Select(x => x.Generate()));
    }
}