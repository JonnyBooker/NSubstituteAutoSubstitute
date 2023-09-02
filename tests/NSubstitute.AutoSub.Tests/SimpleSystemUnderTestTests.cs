using AutoFixture;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class SimpleSystemUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();
    private Fixture Fixture { get; } = new();

    [Fact]
    public void SimpleSystemUnderTest_WhenCreatedDuringAct_WillReturnExpectedResult()
    {
        //Arrange
        var stringValue = Fixture.Create<string>();

        AutoSubstitute
            .SubstituteFor<IStringGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = sut.UppercaseStringGenerationResult();

        //Assert
        Assert.Equal(stringValue.ToUpper(), result);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenCreatedDuringArrange_WillReturnExpectedResult()
    {
        //Arrange
        var sut = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var stringValue = Fixture.Create<string>();

        AutoSubstitute
            .SubstituteFor<IStringGenerationDependency>()
            .Generate()
            .Returns(stringValue);

        //Act
        var result = sut.UppercaseStringGenerationResult();

        //Assert
        Assert.Equal(stringValue.ToUpper(), result);
    }

    [Fact]
    public void EmptySystemUnderTest_WhenUsed_ReturnsExpectedResult()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<EmptySystemUnderTest>();
        var result = sut.Generate();

        //Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void EnumerableSystemUnderTest_WhenMockedEnumerableOfDependencies_ReturnsExpectedResult()
    {
        //Arrange
        var item1 = Fixture.Create<string>();
        var item2 = Fixture.Create<string>();

        var instance1 = AutoSubstitute.SubstituteFor<IStringGenerationDependency>(true);
        var instance2 = AutoSubstitute.SubstituteFor<IStringGenerationDependency>(true);

        instance1
            .Generate()
            .Returns(item1);
        instance2
            .Generate()
            .Returns(item2);
        
        AutoSubstitute.UseSubstituteCollection(instance1, instance2);

        var sut = AutoSubstitute.CreateInstance<EnumerableSystemUnderTest>();
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item1} {item2}", result);
    }
}

public class EmptySystemUnderTest
{
    public string Generate()
    {
        return "Hello World";
    }
}

public class EnumerableSystemUnderTest
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

public class SimpleSystemUnderTest
{
    private readonly IStringGenerationDependency _stringGenerationDependency;
    private readonly IIntGenerationDependency _intGenerationDependency;

    public SimpleSystemUnderTest(IStringGenerationDependency stringGenerationDependency, IIntGenerationDependency intGenerationDependency)
    {
        _stringGenerationDependency = stringGenerationDependency;
        _intGenerationDependency = intGenerationDependency;
    }

    public string UppercaseStringGenerationResult()
    {
        var value = _stringGenerationDependency.Generate();
        return value.ToUpper();
    }

    public int NegateIntGenerationResult()
    {
        var value = _intGenerationDependency.Generate();
        return -value;
    }

    public string CombineIntAndStringGeneration()
    {
        var stringValue = _stringGenerationDependency.Generate();
        var intValue = _intGenerationDependency.Generate();

        return $"{stringValue} {intValue}";
    }
}

public class HelloStringGenerationDependency : IStringGenerationDependency
{
    public string Generate()
    {
        return "Hello";
    }
}

public class WorldStringGenerationDependency : IStringGenerationDependency
{
    public string Generate()
    {
        return "World";
    }
}

public class EvenIntGenerationDependency : IIntGenerationDependency
{
    public int Generate()
    {
        return 10;
    }
}

public class OddIntGenerationDependency : IIntGenerationDependency
{
    public int Generate()
    {
        return 5;
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
