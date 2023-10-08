using AutoFixture;
using NSubstitute.AutoSub.Exceptions;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class PrivateConstructorTests
{
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void PrivateConstructor_WhenCalledWithoutUsePrivateConstructorsFlag_WillThrowAutoSubstituteException()
    {
        //Arrange
        const string expectedException =  "Unable to find suitable constructor. Ensure there is a constructor that is accessible (i.e. public) and its constructor parameter types are also accessible. Alternatively, you can use 'usePrivateConstructors' when 'AutoSubstitute' is created.";

        var autoSubstitute = new AutoSubstitute();

        //Act & Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            autoSubstitute.CreateInstance<PrivateConstructorSystemUnderTest>();
        });
        
        Assert.Equal(expectedException, exception.Message);
    }
    
    [Fact]
    public void PrivateConstructor_WhenCalledWithUsePrivateConstructorsFlag_WillReturnExpectedResult()
    {
        //Arrange
        var expectedText = Fixture.Create<string>();
        var autoSubstitute = new AutoSubstitute(usePrivateConstructors: true);

        autoSubstitute
            .SubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(expectedText);

        //Act & Assert
        var sut = autoSubstitute.CreateInstance<PrivateConstructorSystemUnderTest>();
        var result = sut.GenerateText();
        
        //Assert
        Assert.Equal(expectedText, result);
    }
    
    [Fact]
    public void PrivateConstructor_WhenCalledWithManualWithNulls_WillThrowAutoSubstituteException()
    {
        //Arrange
        const string expectedException =  "Unable to find suitable constructor. You are using 'Manual with Nulls' behaviour mode, a mock must be created for the method before the system under test instance is created. Alternatively, use an 'Automatic' behaviour mode or you can use 'usePrivateConstructors' when 'AutoSubstitute' is created.";

        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);

        //Act & Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            autoSubstitute.CreateInstance<PrivateConstructorSystemUnderTest>();
        });
        
        Assert.Equal(expectedException, exception.Message);
    }
    
    [Fact]
    public void PrivateConstructor_WhenCalledWithManualWithExceptions_WillThrowAutoSubstituteException()
    {
        //Arrange
        const string expectedException =  "Unable to find suitable constructor. You are using 'Manual with Exceptions' behaviour mode, the type you are create as a system under test may contain concrete implementations that are not suitable for this behaviour mode. 'Use' can be used to ensure that these classes get the implementations that are expected to work with this behaviour mode. Alternatively, use an 'Automatic' behaviour mode or you can use 'usePrivateConstructors' when 'AutoSubstitute' is created.";

        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);

        //Act & Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            autoSubstitute.CreateInstance<PrivateConstructorSystemUnderTest>();
        });
        
        Assert.Equal(expectedException, exception.Message);
    }

    private class PrivateConstructorSystemUnderTest
    {
        private readonly ITextGenerationDependency _textGenerationDependency;

        private PrivateConstructorSystemUnderTest(ITextGenerationDependency textGenerationDependency)
        {
            _textGenerationDependency = textGenerationDependency;
        }

        public string GenerateText()
        {
            return _textGenerationDependency.Generate();
        }
    }
    
    public interface ITextGenerationDependency
    {
        string Generate();
    }
}