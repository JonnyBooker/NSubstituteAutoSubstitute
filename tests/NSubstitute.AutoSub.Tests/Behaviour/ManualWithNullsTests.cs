using AutoFixture;
using NSubstitute.AutoSub.Tests.Behaviour.Dependencies;
using NSubstitute.AutoSub.Tests.Behaviour.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.Behaviour;

public class ManualWithNullsTests
{
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndOnlyDependencyMockedPriorToCreateUsed_HasExpectedResult()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);

        autoSubstitute
            .SubstituteFor<IBehaviourTextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();
        var result = instance.Generate();

        //Assert
        Assert.Equal(expectedValue, result);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndAllDependenciesMockedPriorToCreate_HasExpectedResult()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);

        autoSubstitute
            .SubstituteFor<IBehaviourTextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);

        autoSubstitute
            .SubstituteFor<IBehaviourNumberGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();
        var result = instance.Generate();

        //Assert
        Assert.Equal(expectedValue, result);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndNoDependencyMockedPriorToCreate_WillThrowNullReferenceException()
    {
        //Arrange
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        Assert.Throws<NullReferenceException>(() =>
        {
            instance.Generate();
        });
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithNulls_WhenUsedAndOnlyOneDependencyMockedPriorToCreate_WillThrowNullReferenceException()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);

        autoSubstitute
            .SubstituteFor<IBehaviourTextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        Assert.Throws<NullReferenceException>(() =>
        {
            instance.CombineTextAndNumberGeneration();
        });
    }
}