using NSubstitute.AutoSub.Tests.For.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class WorldStringGenerationDependency : IStringGenerationDependency
{
    public const string WorldText = "World";
    
    public string Generate()
    {
        return WorldText;
    }
}