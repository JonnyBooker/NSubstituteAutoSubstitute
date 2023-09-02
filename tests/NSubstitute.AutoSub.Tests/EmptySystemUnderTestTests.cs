using AutoFixture;
using NSubstitute.AutoSub.Tests.TestClasses;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class EmptySystemUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();
    
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void EmptySystemUnderTest_WhenUsedWithNoMockedDependencies_ReturnsExpectedResult()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<EmptySystemUnderTest>();
        var result = sut.Generate();

        //Assert
        Assert.Equal(EmptySystemUnderTest.GenerateText, result);
    }
}