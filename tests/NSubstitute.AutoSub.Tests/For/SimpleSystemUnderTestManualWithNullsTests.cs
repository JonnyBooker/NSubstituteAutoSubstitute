using AutoFixture;
using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.For;

public class SimpleSystemUnderTestManualWithNullsTests
{
    private AutoSubstitute AutoSubstitute { get; } = new(SubstituteBehaviour.ManualWithNulls);
    
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndOnlyDependencyMockedPriorToCreateUsed_HasExpectedResult()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();

        AutoSubstitute
            .SubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = instance.GenerateText();

        //Assert
        Assert.Equal(expectedValue, result);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndAllDependenciesMockedPriorToCreate_HasExpectedResult()
    {
        //Arrange
        var expectedTextValue = Fixture.Create<string>();
        var expectedNumberValue = Fixture.Create<int>();
        
        AutoSubstitute
            .SubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(expectedTextValue);

        AutoSubstitute
            .SubstituteFor<INumberGenerationDependency>()
            .Generate()
            .Returns(expectedNumberValue);
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = instance.CombineTextAndNumberGeneration();

        //Assert
        Assert.Equal($"{expectedTextValue} {expectedNumberValue}", result);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndNoDependencyMockedPriorToCreate_WillThrowNullReferenceException()
    {
        //Arrange & Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

        //Assert
        Assert.Throws<NullReferenceException>(() =>
        {
            instance.GenerateText();
        });
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndOnlyOneDependencyMockedPriorToCreate_WillThrowNullReferenceException()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();
        
        AutoSubstitute
            .SubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

        //Assert
        Assert.Throws<NullReferenceException>(() =>
        {
            instance.CombineTextAndNumberGeneration();
        });
    }
}