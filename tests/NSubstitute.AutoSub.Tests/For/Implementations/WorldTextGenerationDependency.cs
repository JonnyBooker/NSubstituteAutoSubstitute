using NSubstitute.AutoSub.Tests.For.Dependencies;

namespace NSubstitute.AutoSub.Tests.For.Implementations;

public class WorldTextGenerationDependency : ITextGenerationDependency
{
    public const string WorldText = "World";

    public string Value => WorldText;

    public string Generate()
    {
        return WorldText;
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