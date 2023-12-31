using AutoFixture;
using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Implementations;
using NSubstitute.AutoSub.Tests.For.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.For;

public class SimpleSystemUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();

    private Fixture Fixture { get; } = new();

    [Fact]
    public void SimpleSystemUnderTest_WhenCreatedDuringAct_WillReturnExpectedMockedResult()
    {
        //Arrange
        var stringValue = Fixture.Create<string>();

        AutoSubstitute
            .GetSubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.GenerateText();

        //Assert
        Assert.Equal(stringValue, result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenCreatedDuringActAndMockMultipleDependencies_WillReturnExpectedMockedResult()
    {
        //Arrange
        var stringValue = Fixture.Create<string>();
        var intValue = Fixture.Create<int>();

        AutoSubstitute
            .GetSubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        AutoSubstitute
            .GetSubstituteFor<INumberGenerationDependency>()
            .Generate()
            .Returns(intValue);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.CombineTextAndNumberGeneration();

        //Assert
        Assert.Equal($"{stringValue} {intValue}", result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenCreatedDuringArrange_WillReturnExpectedMockedResult()
    {
        //Arrange
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var stringValue = Fixture.Create<string>();

        AutoSubstitute
            .GetSubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var result = sut.GenerateText();

        //Assert
        Assert.Equal(stringValue, result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenCreatedDuringArrangeAndMockMultipleDependencies_WillReturnExpectedMockedResult()
    {
        //Arrange
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var stringValue = Fixture.Create<string>();
        var intValue = Fixture.Create<int>();

        AutoSubstitute
            .GetSubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        AutoSubstitute
            .GetSubstituteFor<INumberGenerationDependency>()
            .Generate()
            .Returns(intValue);

        //Act
        var result = sut.CombineTextAndNumberGeneration();

        //Assert
        Assert.Equal($"{stringValue} {intValue}", result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenGivenImplementation_WillReturnImplementationResult()
    {
        //Arrange
        AutoSubstitute.Use<ITextGenerationDependency>(new HelloTextGenerationDependency());

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.GenerateText();

        //Assert
        Assert.Equal(HelloTextGenerationDependency.HelloText, result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenUsingNoTrackingSubstitute_WillNotUseMockedVersion()
    {
        //Arrange
        var stringValue = Fixture.Create<string>();

        AutoSubstitute
            .GetSubstituteForNoTracking<ITextGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.GenerateText();

        //Assert
        Assert.Empty(result);
    }
}