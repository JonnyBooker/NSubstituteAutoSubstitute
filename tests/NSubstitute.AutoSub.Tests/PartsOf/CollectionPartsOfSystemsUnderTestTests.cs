using AutoFixture;
using NSubstitute.AutoSub.Tests.PartsOf.Systems;
using NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections;
using NSubstitute.AutoSub.Tests.PartsOf.Systems.Collections.Interfaces;
using Xunit;

namespace NSubstitute.AutoSub.Tests.PartsOf;

public class CollectionPartsOfSystemsUnderTestTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();

    private Fixture Fixture { get; } = new();
    
    public static IEnumerable<object[]> CollectionData => new List<object[]>
    {
        new object[] { typeof(ReadOnlyPartsOfSystemUnderTest) },
        new object[] { typeof(EnumerablePartsOfSystemUnderTest) },
        new object[] { typeof(ListPartsOfSystemUnderTest) },
        new object[] { typeof(CollectionPartsOfSystemUnderTest) }
    };
    
    [Theory]
    [MemberData(nameof(CollectionData))]
    public void CollectionSystemUnderTestInstances_WhenMultipleDependenciesMocked_ReturnsAllSubstitutedMockedValues(Type value)
    {
        //Arrange
        var item1 = Fixture.Create<string>();
        var item2 = Fixture.Create<string>();

        var mockInstance1 = AutoSubstitute.SubstituteForPartsOfNoCache<SimplePartsOfPartsOfDependency>();
        var mockInstance2 = AutoSubstitute.SubstituteForPartsOfNoCache<SimplePartsOfPartsOfDependency>();

        mockInstance1
            .PartsOfInvoke()
            .Returns(item1);
        mockInstance2
            .PartsOfInvoke()
            .Returns(item2);
        
        AutoSubstitute.UseCollection(mockInstance1, mockInstance2);

        var sut = (ICollectionPartsOfSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item1} {item2}", result);
    }

    [Theory]
    [MemberData(nameof(CollectionData))]
    public void CollectionSystemUnderTest_WhenUsingSubstituteForEnumerableOnce_ReturnsOnlySubstitutedMockedValue(Type value)
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();

        var mockInstance = AutoSubstitute.SubstituteForPartsOf<SimplePartsOfPartsOfDependency>();

        mockInstance
            .PartsOfInvoke()
            .Returns(expectedValue);

        //Act
        var sut = (ICollectionPartsOfSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.NotEqual(result, SimplePartsOfPartsOfDependency.NonMockedText);
        Assert.Equal(expectedValue, result);
    }
}