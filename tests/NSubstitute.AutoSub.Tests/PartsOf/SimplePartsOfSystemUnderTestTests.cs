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
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<SimplePartsOfSystemUnderTest>();
        var result = sut.Invoke();

        //Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void SimplePartsOfSystemUnderTest_WhenCallDependencyMethodThatIsMocked_ReturnsMockedValue()
    {
        //Arrange
        var value = Fixture.Create<string>();
        
        AutoSubstitute
            .GetSubstituteForPartsOf<SimplePartsOfPartsOfDependency>()
            .PartsOfInvoke()
            .Returns(value);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimplePartsOfSystemUnderTest>();
        var result = sut.Invoke();

        //Assert
        Assert.NotEqual(result, SimplePartsOfPartsOfDependency.NonMockedText);
        Assert.Equal(result, value);
    }
}