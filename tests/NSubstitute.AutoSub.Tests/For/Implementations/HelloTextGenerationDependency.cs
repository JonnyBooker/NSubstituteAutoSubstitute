using NSubstitute.AutoSub.Tests.For.Dependencies;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class HelloTextGenerationDependency : ITextGenerationDependency
{
    public const string HelloText = "Hello";

    public string Value => HelloText;

    public string Generate()
    {
        return HelloText;
    }

    public string Combine(string prefix, string postfix)
    {
        return $"{prefix}{postfix}";
    }

    public void Process()
    {
        //Nothing to see here
    }
}