using NSubstitute.AutoSub.Tests.For.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class HelloStringGenerationDependency : IStringGenerationDependency
{
    public const string HelloText = "Hello";
    
    public string Generate()
    {
        return HelloText;
    }
}