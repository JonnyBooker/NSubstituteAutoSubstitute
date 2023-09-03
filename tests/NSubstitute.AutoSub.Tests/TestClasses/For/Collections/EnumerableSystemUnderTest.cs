using NSubstitute.AutoSub.Tests.TestClasses.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;

namespace NSubstitute.AutoSub.Tests.TestClasses.Collections;

public class EnumerableSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IEnumerable<IStringGenerationDependency> _stringGenerationDependencies;

    public EnumerableSystemUnderTest(IEnumerable<IStringGenerationDependency> stringGenerationDependencies)
    {
        _stringGenerationDependencies = stringGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _stringGenerationDependencies.Select(x => x.Generate()));
    }
}