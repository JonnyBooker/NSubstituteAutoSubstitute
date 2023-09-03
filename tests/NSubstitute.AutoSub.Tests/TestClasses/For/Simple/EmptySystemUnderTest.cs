namespace NSubstitute.AutoSub.Tests.TestClasses.Simple;

public class EmptySystemUnderTest
{
    public const string GenerateText = "Hello World";
    
    public string Generate()
    {
        return GenerateText;
    }
}