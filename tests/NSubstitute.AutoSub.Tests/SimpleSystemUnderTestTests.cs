using AutoFixture;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

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
        AutoSubstitute.UseSubstitute<IStringGenerationDependency>(new HelloStringGenerationDependency());

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.StringGenerationResult();

        //Assert
        Assert.Equal(HelloStringGenerationDependency.HelloText, result);
    }

    [Fact]
    public void EmptySystemUnderTest_WhenUsedWithNoMockedDependencies_ReturnsExpectedResult()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<EmptySystemUnderTest>();
        var result = sut.Generate();

        //Assert
        Assert.Equal(EmptySystemUnderTest.GenerateText, result);
    }

    [Theory]
    [InlineData(typeof(ReadOnlyCollectionSystemUnderTest))]
    [InlineData(typeof(EnumerableSystemUnderTest))]
    public void EnumerableSystemUnderTest_WhenMockedEnumerableOfDependencies_ReturnsAllSubstitutedMockedValues(Type value)
    {
        //Arrange
        var item1 = Fixture.Create<string>();
        var item2 = Fixture.Create<string>();

        var instance1 = AutoSubstitute.SubstituteForNoCache<IStringGenerationDependency>();
        var instance2 = AutoSubstitute.SubstituteForNoCache<IStringGenerationDependency>();

        instance1
            .Generate()
            .Returns(item1);
        instance2
            .Generate()
            .Returns(item2);
        
        AutoSubstitute.UseSubstituteCollection(instance1, instance2);

        var sut = (ICollectionSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item1} {item2}", result);
    }

    [Fact]
    public void EnumerableSystemUnderTest_WhenUsingSubstituteForEnumerableOnce_ReturnsOnlySubstitutedMockedValue()
    {
        //Arrange
        var item1 = Fixture.Create<string>();

        var instance = AutoSubstitute.SubstituteFor<IStringGenerationDependency>();

        instance
            .Generate()
            .Returns(item1);

        //Act
        var sut = AutoSubstitute.CreateInstance<EnumerableSystemUnderTest>();
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item1}", result);
    }
}

public class EmptySystemUnderTest
{
    public const string GenerateText = "Hello World";
    
    public string Generate()
    {
        return GenerateText;
    }
}

public interface ICollectionSystemUnderTest
{
    string Generate();
}

public class EnumerableSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IEnumerable<IStringGenerationDependency> _stringGenerationDependencies;

    public EnumerableSystemUnderTest(IEnumerable<IStringGenerationDependency> stringGenerationDependencies)
    {
        _stringGenerationDependencies = stringGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _stringGenerationDependencies.Select(x => x.Generate()));
    }
}

public class ReadOnlyCollectionSystemUnderTest : ICollectionSystemUnderTest
{
    private readonly IReadOnlyCollection<IStringGenerationDependency> _stringGenerationDependencies;

    public ReadOnlyCollectionSystemUnderTest(IReadOnlyCollection<IStringGenerationDependency> stringGenerationDependencies)
    {
        _stringGenerationDependencies = stringGenerationDependencies;
    }

    public string Generate()
    {
        return string.Join(" ", _stringGenerationDependencies.Select(x => x.Generate()));
    }
}

public class SimpleSystemUnderTest
{
    private readonly IStringGenerationDependency _stringGenerationDependency;
    private readonly IIntGenerationDependency _intGenerationDependency;

    public SimpleSystemUnderTest(IStringGenerationDependency stringGenerationDependency, IIntGenerationDependency intGenerationDependency)
    {
        _stringGenerationDependency = stringGenerationDependency;
        _intGenerationDependency = intGenerationDependency;
    }

    public string StringGenerationResult()
    {
        var value = _stringGenerationDependency.Generate();
        return value;
    }

    public string CombineStringAndIntGeneration()
    {
        var stringValue = _stringGenerationDependency.Generate();
        var intValue = _intGenerationDependency.Generate();

        return $"{stringValue} {intValue}";
    }
}

public class HelloStringGenerationDependency : IStringGenerationDependency
{
    public const string HelloText = "Hello";
    
    public string Generate()
    {
        return HelloText;
    }
}

public class WorldStringGenerationDependency : IStringGenerationDependency
{
    public const string WorldText = "World";
    
    public string Generate()
    {
        return WorldText;
    }
}

public interface IStringGenerationDependency
{
    string Generate();
}

public interface IIntGenerationDependency
{
    int Generate();
}