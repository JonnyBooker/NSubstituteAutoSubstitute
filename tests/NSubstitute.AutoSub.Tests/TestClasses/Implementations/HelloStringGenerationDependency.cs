using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;

namespace NSubstitute.AutoSub.Tests.TestClasses.Implementations;

public class HelloStringGenerationDependency : IStringGenerationDependency
{
    public const string HelloText = "Hello";
    
    public string Generate()
    {
        return HelloText;
    }
}