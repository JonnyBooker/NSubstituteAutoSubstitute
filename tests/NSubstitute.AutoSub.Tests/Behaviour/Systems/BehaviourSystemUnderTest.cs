using NSubstitute.AutoSub.Tests.Behaviour.Dependencies;

namespace NSubstitute.AutoSub.Tests.Behaviour.Systems;

public class BehaviourSystemUnderTest
{
    private readonly IBehaviourTextGenerationDependency _textGenerationDependency;
    private readonly IBehaviourNumberGenerationDependency _numberGenerationDependency;

    public BehaviourSystemUnderTest(IBehaviourTextGenerationDependency textGenerationDependency, IBehaviourNumberGenerationDependency numberGenerationDependency)
    {
        _textGenerationDependency = textGenerationDependency;
        _numberGenerationDependency = numberGenerationDependency;
    }

    public string Generate()
    {
        var value = _textGenerationDependency.Generate();
        return value;
    }

    public string CombineTextAndNumberGeneration()
    {
        var stringValue = _textGenerationDependency.Generate();
        var intValue = _numberGenerationDependency.Generate();

        return $"{stringValue} {intValue}";
    }

    public string GetValue()
    {
        return _textGenerationDependency.Value;
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