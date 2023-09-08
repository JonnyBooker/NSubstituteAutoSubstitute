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
            .SubstituteFor<IStringGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.StringGenerationResult();

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
            .SubstituteFor<IStringGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        AutoSubstitute
            .SubstituteFor<IIntGenerationDependency>()
            .Generate()
            .Returns(intValue);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.CombineStringAndIntGeneration();

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
            .SubstituteFor<IStringGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var result = sut.StringGenerationResult();

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
            .SubstituteFor<IStringGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        AutoSubstitute
            .SubstituteFor<IIntGenerationDependency>()
            .Generate()
            .Returns(intValue);

        //Act
        var result = sut.CombineStringAndIntGeneration();

        //Assert
        Assert.Equal($"{stringValue} {intValue}", result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenGivenImplementation_WillReturnImplementationResult()
    {
        //Arrange
        AutoSubstitute.Use<IStringGenerationDependency>(new HelloStringGenerationDependency());

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.StringGenerationResult();

        //Assert
        Assert.Equal(HelloStringGenerationDependency.HelloText, result);
    }
}