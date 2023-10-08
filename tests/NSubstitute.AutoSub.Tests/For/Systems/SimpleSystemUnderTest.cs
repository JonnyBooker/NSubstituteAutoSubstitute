using NSubstitute.AutoSub.Tests.For.Dependencies;

namespace NSubstitute.AutoSub.Tests.For.Systems;

public class SimpleSystemUnderTest
{
    private readonly ITextGenerationDependency _textGenerationDependency;
    private readonly INumberGenerationDependency _numberGenerationDependency;

    public SimpleSystemUnderTest(ITextGenerationDependency textGenerationDependency, INumberGenerationDependency numberGenerationDependency)
    {
        _textGenerationDependency = textGenerationDependency;
        _numberGenerationDependency = numberGenerationDependency;
    }

    public string GetValue()
    {
        return _textGenerationDependency.Value;
    }

    public string GenerateText()
    {
        var value = _textGenerationDependency.Generate();
        return value;
    }

    public string CombineTextAndNumberGeneration()
    {
        var textValue = _textGenerationDependency.Generate();
        var numberValue = _numberGenerationDependency.Generate();

        return $"{textValue} {numberValue}";
    }

    public string CombinePreAndPostStrings(string prefix, string postfix)
    {
        return _textGenerationDependency.Combine(prefix, postfix);
    }

    public void Process()
    {
        _textGenerationDependency.Process();
    }
}