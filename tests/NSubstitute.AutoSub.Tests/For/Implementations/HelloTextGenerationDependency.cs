using NSubstitute.AutoSub.Tests.For.Dependencies;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class HelloTextGenerationDependency : ITextGenerationDependency
{
    public const string HelloText = "Hello";
    
    public string Generate()
    {
        return HelloText;
    }
}