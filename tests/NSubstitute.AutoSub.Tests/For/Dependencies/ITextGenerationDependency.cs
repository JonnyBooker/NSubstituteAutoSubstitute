namespace NSubstitute.AutoSub.Tests.For.Dependencies;

public interface ITextGenerationDependency
{
    string Value { get; }
    
    string Generate();
    
    string Combine(string prefix, string postfix);
    
    void Process();
}