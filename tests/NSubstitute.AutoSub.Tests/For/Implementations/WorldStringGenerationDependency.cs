using NSubstitute.AutoSub.Tests.For.Dependencies;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class WorldStringGenerationDependency : IStringGenerationDependency
{
    public const string WorldText = "World";
    
    public string Generate()
    {
        return WorldText;
    }
}