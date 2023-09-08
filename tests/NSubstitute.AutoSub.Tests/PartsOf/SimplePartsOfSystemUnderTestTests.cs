using AutoFixture;
using NSubstitute.AutoSub.Tests.PartsOf.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.PartsOf;

public class SimplePartsOfSystemUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();

    private Fixture Fixture { get; } = new();

    [Fact]
    public void SimplePartsOfSystemUnderTest_WhenCallDependencyMethodThatIsNotMocked_ReturnsOriginalValue()
    {
        //Arrange
        AutoSubstitute
            .SubstituteForPartsOf<SimplePartsOfPartsOfDependency>();

        //Act
        var sut = AutoSubstitute.CreateInstance<SimplePartsOfSystemUnderTest>();
        var result = sut.InvokeNotMocked();

        //Assert
        Assert.Equal(result, SimplePartsOfPartsOfDependency.NonMockedText);
    }
    
    [Fact]
    public void SimplePartsOfSystemUnderTest_WhenCallDependencyMethodThatIsMocked_ReturnsMockedValue()
    {
        //Arrange
        var value = Fixture.Create<string>();
        
        AutoSubstitute
            .SubstituteForPartsOf<SimplePartsOfPartsOfDependency>()
            .MockedMethod()
            .Returns(value);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimplePartsOfSystemUnderTest>();
        var result = sut.InvokeMocked();

        //Assert
        Assert.NotEqual(result, SimplePartsOfPartsOfDependency.MockedText);
        Assert.Equal(result, value);
    }
}