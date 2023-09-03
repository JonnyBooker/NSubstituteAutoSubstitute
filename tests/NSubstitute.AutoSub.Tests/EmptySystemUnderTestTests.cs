using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class EmptySystemUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();
    
    [Fact]
    public void EmptySystemUnderTest_WhenUsedWithNoMockedDependencies_ReturnsExpectedResult()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<EmptySystemUnderTest>();
        var result = sut.Generate();

        //Assert
        Assert.Equal(EmptySystemUnderTest.GenerateText, result);
    }
    
    private class EmptySystemUnderTest
    {
        public const string GenerateText = "Hello World";
    
        public string Generate()
        {
            return GenerateText;
        }
    }
}