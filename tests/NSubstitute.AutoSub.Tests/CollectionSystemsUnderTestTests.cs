using AutoFixture;
using NSubstitute.AutoSub.Tests.TestClasses.Collections;
using NSubstitute.AutoSub.Tests.TestClasses.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.TestClasses.Implementations;
using NSubstitute.AutoSub.Tests.TestClasses.Interfaces;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class CollectionSystemsUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();

    private Fixture Fixture { get; } = new();
    
    [Theory]
    [InlineData(typeof(ReadOnlyCollectionSystemUnderTest))]
    [InlineData(typeof(EnumerableSystemUnderTest))]
    [InlineData(typeof(ListCollectionSystemUnderTest))]
    [InlineData(typeof(CollectionSystemUnderTest))]
    public void CollectionSystemUnderTestInstances_WhenMultipleDependenciesMocked_ReturnsAllSubstitutedMockedValues(Type value)
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

    [Theory]
    [InlineData(typeof(ReadOnlyCollectionSystemUnderTest))]
    [InlineData(typeof(EnumerableSystemUnderTest))]
    [InlineData(typeof(ListCollectionSystemUnderTest))]
    [InlineData(typeof(CollectionSystemUnderTest))]
    public void CollectionSystemUnderTest_WhenUsingSubstituteForEnumerableOnce_ReturnsOnlySubstitutedMockedValue(Type value)
    {
        //Arrange
        var item = Fixture.Create<string>();

        var instance = AutoSubstitute.SubstituteFor<IStringGenerationDependency>();

        instance
            .Generate()
            .Returns(item);

        //Act
        var sut = (ICollectionSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item}", result);
    }
    
    
    [Theory]
    [InlineData(typeof(ReadOnlyCollectionSystemUnderTest))]
    [InlineData(typeof(EnumerableSystemUnderTest))]
    [InlineData(typeof(ListCollectionSystemUnderTest))]
    [InlineData(typeof(CollectionSystemUnderTest))]
    public void SimpleSystemUnderTest_WhenGivenMultipleImplementations_WillReturnImplementationsResult(Type value)
    {
        //Arrange
        AutoSubstitute.UseSubstituteCollection<IStringGenerationDependency>(new HelloStringGenerationDependency(),
            new WorldStringGenerationDependency());

        //Act
        var sut = (ICollectionSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{HelloStringGenerationDependency.HelloText} {WorldStringGenerationDependency.WorldText}", result);
    }
}