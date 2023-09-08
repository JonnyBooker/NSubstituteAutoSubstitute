using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Systems.Collections;

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