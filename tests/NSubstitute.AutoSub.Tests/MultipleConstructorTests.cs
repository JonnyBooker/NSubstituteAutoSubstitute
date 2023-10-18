using AutoFixture;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class MultipleConstructorTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();
    
    private Fixture Fixture { get; } = new();

    [Fact]
    public void GreedyConstructorSystemUnderTest_WhenCalled_SelectsLargestConstructor()
    {
        //Arrange
        var text = Fixture.Create<string>();
        var number = Fixture.Create<int>();

        AutoSubstitute
            .GetSubstituteFor<ITextGenerationDependency>()
            .Generate()
            .Returns(text);

        AutoSubstitute
            .GetSubstituteFor<INumberGenerationDependency>()
            .Generate()
            .Returns(number);
        
        //Act
        var sut = AutoSubstitute.CreateInstance<GreedyConstructorSystemUnderTest>();
        var result = sut.CombineTextAndNumberDependency();

        //Assert
        Assert.Equal($"{text} {number}", result);
    }

    private class GreedyConstructorSystemUnderTest
    {
        private readonly ITextGenerationDependency _textGenerationDependency;
        private readonly INumberGenerationDependency _numberGenerationDependency;

        public GreedyConstructorSystemUnderTest(INumberGenerationDependency numberGenerationDependency)
        {
            _numberGenerationDependency = numberGenerationDependency;
        }
        
        public GreedyConstructorSystemUnderTest(ITextGenerationDependency textGenerationDependency)
        {
            _textGenerationDependency = textGenerationDependency;
        }

        public GreedyConstructorSystemUnderTest(ITextGenerationDependency textGenerationDependency, INumberGenerationDependency numberGenerationDependency)
        {
            _textGenerationDependency = textGenerationDependency;
            _numberGenerationDependency = numberGenerationDependency;
        }

        public string CombineTextAndNumberDependency()
        {
            return $"{_textGenerationDependency.Generate()} {_numberGenerationDependency.Generate()}";
        }
    }
    
    public interface ITextGenerationDependency
    {
        string Generate();
    }
    
    public interface INumberGenerationDependency
    {
        int Generate();
    }
}