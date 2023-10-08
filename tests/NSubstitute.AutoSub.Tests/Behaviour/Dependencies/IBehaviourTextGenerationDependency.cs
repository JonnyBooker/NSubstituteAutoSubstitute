namespace NSubstitute.AutoSub.Tests.Behaviour.Dependencies;

public interface IBehaviourTextGenerationDependency
{
    string Value { get; }
    
    string Generate();

    string Combine(string prefix, string postfix);

    void Process();
}