using AutoFixture;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.For;

public class SimpleSystemUnderTestManualWithExceptionsTests
{
    private AutoSubstitute AutoSubstitute { get; } = new(SubstituteBehaviour.ManualWithExceptions);
    
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedAndOnlyDependencyMockedPriorToCreateUsed_HasExpectedResult()
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
    public void SubstituteBehaviourManualWithExceptions_WhenUsedAndAllDependenciesMockedPriorToCreate_HasExpectedResult()
    {
        //Arrange
        var expectedTextValue = Fixture.Create<string>();
        var expectedIntValue = Fixture.Create<int>();
        
        AutoSubstitute
            .SubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(expectedTextValue);

        AutoSubstitute
            .SubstituteFor<INumberGenerationDependency>()
            .Generate()
            .Returns(expectedIntValue);
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();
        var result = instance.CombineTextAndNumberGeneration();

        //Assert
        Assert.Equal($"{expectedTextValue} {expectedIntValue}", result);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedOnMethodWithSingleParameterAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'ITextGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.GenerateText();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
    
    [Fact]
    public void SubstituteBehaviourManualWithExceptions_WhenUsedOnMethodWithMultipleParametersAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var expectedMessage = "Mock has not been configured for 'ITextGenerationDependency' when method 'Combine' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

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
        var expectedMessage = "Mock has not been configured for 'ITextGenerationDependency' when method 'Process' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

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
        var expectedMessage = "Mock has not been configured for 'INumberGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        var expectedValue = Fixture.Create<string>();
        
        AutoSubstitute
            .SubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(expectedValue);
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

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
        var expectedMessage = "Mock has not been configured for 'ITextGenerationDependency' when property 'Value' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
        
        //Act
        var instance = AutoSubstitute.CreateInstance<SimpleSystemUnderTest>();

        //Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            instance.GetValue();
        });
        
        Assert.Equal(expectedMessage, exception.Message);
    }
}