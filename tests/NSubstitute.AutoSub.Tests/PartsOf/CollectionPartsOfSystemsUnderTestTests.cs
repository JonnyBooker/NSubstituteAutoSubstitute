using AutoFixture;
using NSubstitute.AutoSub.Tests.PartsOf.Collections;
using NSubstitute.AutoSub.Tests.PartsOf.Collections.Interfaces;
using NSubstitute.AutoSub.Tests.PartsOf.Simple;
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

        var instance1 = AutoSubstitute.SubstituteForPartsOfNoCache<SimplePartsOfPartsOfDependency>();
        var instance2 = AutoSubstitute.SubstituteForPartsOfNoCache<SimplePartsOfPartsOfDependency>();

        instance1
            .MockedMethod()
            .Returns(item1);
        instance2
            .MockedMethod()
            .Returns(item2);
        
        AutoSubstitute.UseSubstituteCollection(instance1, instance2);

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
        var item = Fixture.Create<string>();

        var instance = AutoSubstitute.SubstituteForPartsOf<SimplePartsOfPartsOfDependency>();

        instance
            .MockedMethod()
            .Returns(item);

        //Act
        var sut = (ICollectionPartsOfSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item}", result);
    }
}