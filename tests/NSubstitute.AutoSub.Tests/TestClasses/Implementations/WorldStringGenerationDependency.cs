using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;

namespace NSubstitute.AutoSub.Tests.TestClasses.Implementations;

public class WorldStringGenerationDependency : IStringGenerationDependency
{
    public const string WorldText = "World";
    
    public string Generate()
    {
        return WorldText;
    }
}