using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems.Collections.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Systems.Collections;

public class CollectionSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly ICollection<ITextGenerationDependency> _textGenerationDependencies;

    public CollectionSystemUnderTest(ICollection<ITextGenerationDependency> textGenerationDependencies)
    {
        _textGenerationDependencies = textGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _textGenerationDependencies.Select(x => x.Generate()));
    }
}