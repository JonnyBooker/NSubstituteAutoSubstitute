using AutoFixture;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Tests.Behaviour.Dependencies;
using NSubstitute.AutoSub.Tests.Behaviour.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.Behaviour;

public class ManualWithExceptionsTests
{
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedAndOnlyDependencyMockedPriorToCreateUsed_HasExpectedResult()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);

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
    public void SubstituteBehaviourManualWithExceptions_WhenUsedAndAllDependenciesMockedPriorToCreate_HasExpectedResult()
    {
        //Arrange
        var expectedValue = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);

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
    public void SubstituteBehaviourManualWithExceptions_WhenUsedOnMethodWithSingleParameterAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'IBehaviourTextGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.Generate();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedOnMethodWithMultipleParametersAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'IBehaviourTextGenerationDependency' when method 'Combine' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.CombinePreAndPostStrings(Fixture.Create<string>(), Fixture.Create<string>());
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedOnMethodWithNoReturnAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'IBehaviourTextGenerationDependency' when method 'Process' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.Process();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedAndOnlyOneDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'IBehaviourNumberGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        var expectedValue = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);

        autoSubstitute
            .SubstituteFor<IBehaviourTextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.CombineTextAndNumberGeneration();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenPropertyCalledOnNonMockedDependency_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'IBehaviourTextGenerationDependency' when property 'Value' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.GetValue();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
}