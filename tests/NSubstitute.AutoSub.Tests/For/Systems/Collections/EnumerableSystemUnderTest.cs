using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Systems.Collections;

public class EnumerableSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IEnumerable<ITextGenerationDependency> _textGenerationDependencies;

    public EnumerableSystemUnderTest(IEnumerable<ITextGenerationDependency> textGenerationDependencies)
    {
        _textGenerationDependencies = textGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _textGenerationDependencies.Select(x => x.Generate()));
    }
}