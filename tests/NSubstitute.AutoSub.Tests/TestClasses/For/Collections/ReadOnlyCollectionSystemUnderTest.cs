using NSubstitute.AutoSub.Tests.TestClasses.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;

namespace NSubstitute.AutoSub.Tests.TestClasses.Collections;

public class ReadOnlyCollectionSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IReadOnlyCollection<IStringGenerationDependency> _stringGenerationDependencies;

    public ReadOnlyCollectionSystemUnderTest(IReadOnlyCollection<IStringGenerationDependency> stringGenerationDependencies)
    {
        _stringGenerationDependencies = stringGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _stringGenerationDependencies.Select(x => x.Generate()));
    }
}