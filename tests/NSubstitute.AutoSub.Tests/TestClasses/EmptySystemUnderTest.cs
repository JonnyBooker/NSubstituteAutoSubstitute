namespace NSubstitute.AutoSub.Tests.TestClasses;

public class EmptySystemUnderTest
{
    public const string GenerateText = "Hello World";
    
    public string Generate()
    {
        return GenerateText;
    }
}