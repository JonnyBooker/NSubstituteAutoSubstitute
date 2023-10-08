using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Systems.Collections;

public class ReadOnlyCollectionSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IReadOnlyCollection<ITextGenerationDependency> _textGenerationDependencies;

    public ReadOnlyCollectionSystemUnderTest(IReadOnlyCollection<ITextGenerationDependency> textGenerationDependencies)
    {
        _textGenerationDependencies = textGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _textGenerationDependencies.Select(x => x.Generate()));
    }
}