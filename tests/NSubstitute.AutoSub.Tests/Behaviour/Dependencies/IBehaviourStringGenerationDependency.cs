namespace NSubstitute.AutoSub.Tests.Behaviour.Dependencies;

public interface IBehaviourStringGenerationDependency
{
    string Value { get; }
    
    string Generate();

    string Combine(string prefix, string postfix);

    void Process();
}