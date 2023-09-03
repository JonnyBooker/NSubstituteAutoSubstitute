using NSubstitute.AutoSub.Tests.For.Interfaces;

namespace NSubstitute.AutoSub.Tests.For.Simple;

public class SimpleSystemUnderTest
{
    private readonly IStringGenerationDependency _stringGenerationDependency;
    private readonly IIntGenerationDependency _intGenerationDependency;

    public SimpleSystemUnderTest(IStringGenerationDependency stringGenerationDependency, IIntGenerationDependency intGenerationDependency)
    {
        _stringGenerationDependency = stringGenerationDependency;
        _intGenerationDependency = intGenerationDependency;
    }

    public string StringGenerationResult()
    {
        var value = _stringGenerationDependency.Generate();
        return value;
    }

    public string CombineStringAndIntGeneration()
    {
        var stringValue = _stringGenerationDependency.Generate();
        var intValue = _intGenerationDependency.Generate();

        return $"{stringValue} {intValue}";
    }
}