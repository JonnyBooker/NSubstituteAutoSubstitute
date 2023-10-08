using NSubstitute.AutoSub.Tests.For.Dependencies;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class WorldTextGenerationDependency : ITextGenerationDependency
{
    public const string WorldText = "World";
    
    public string Generate()
    {
        return WorldText;
    }
}