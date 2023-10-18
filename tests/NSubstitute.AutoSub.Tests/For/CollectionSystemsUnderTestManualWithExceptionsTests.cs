using AutoFixture;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems.Collections;
using NSubstitute.AutoSub.Tests.For.Systems.Collections.Interfaces;
using Xunit;

namespace NSubstitute.AutoSub.Tests.For;

public class CollectionSystemsUnderTestManualWithExceptionsTests
{
    private AutoSubstitute AutoSubstitute { get; } = new(SubstituteBehaviour.ManualWithExceptions);
    
    private Fixture Fixture { get; } = new();
    
    public static IEnumerable<object[]> CollectionData => new List<object[]>
    {
        new object[] { typeof(ReadOnlyCollectionSystemUnderTest) },
        new object[] { typeof(EnumerableSystemUnderTest) },
        new object[] { typeof(ListCollectionSystemUnderTest) },
        new object[] { typeof(CollectionSystemUnderTest) }
    };

    [Theory]
    [MemberData(nameof(CollectionData))]
    public void CollectionSystemUnderTestInstances_WhenMultipleDependenciesMocked_ReturnsAllSubstitutedMockedValues(Type value)
    {
        //Arrange
        var item1 = Fixture.Create<string>();
        var item2 = Fixture.Create<string>();

        var instance1 = AutoSubstitute.GetSubstituteForNoTracking<ITextGenerationDependency>();
        var instance2 = AutoSubstitute.GetSubstituteForNoTracking<ITextGenerationDependency>();

        instance1
            .Generate()
            .Returns(item1);
        instance2
            .Generate()
            .Returns(item2);
        
        AutoSubstitute.UseCollection(instance1, instance2);

        var sut = (ICollectionSystemUnderTest) AutoSubstitute.CreateInstance(value);
        var result = sut.Generate();

        //Assert
        Assert.Equal($"{item1} {item2}", result);
    }

    [Theory]
    [MemberData(nameof(CollectionData))]
    public void CollectionSystemUnderTestInstances_WhenUsedAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException(Type value)
    {
        //Arrange
        var expectedMessage = "Substitute has not been configured for 'ITextGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the substitute must be created before 'CreateInstance' is called.";
        
        //Act
        var sut = (ICollectionSystemUnderTest) AutoSubstitute.CreateInstance(value);

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            sut.Generate();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
}