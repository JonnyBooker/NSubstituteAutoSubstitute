using AutoFixture;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class ReceivedHelperTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();
    
    private Fixture Fixture { get; } = new();
    
    [Fact]
    public void ReceivedOnce_WhenCalledWithNoParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.StringGenerationResult();

        //Assert
        AutoSubstitute
            .ReceivedOnce<IReceivedHelperTestClassDependency>(x => x.Generate());
    }
    
    [Fact]
    public void ReceivedOnce_WhenCalledWithParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange 
        var parameter = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithStringGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .ReceivedOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter));
    }
    
    [Fact]
    public void ReceivedOnce_WhenCalledMultipleTimes_CanChainVerify()
    {
        //Arrange 
        var parameter1 = Fixture.Create<string>();
        var parameter2 = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithStringGenerationResult(parameter1);
        _ = sut.CombineWithStringGenerationResult(parameter2);

        //Assert
        AutoSubstitute
            .ReceivedOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter1))
            .ReceivedOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter2));
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledWithNoParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.StringGenerationResult();

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.Generate(), 1);
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledWithParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange 
        var parameter = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithStringGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter), 1);
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledMultipleTimes_CanChainVerify()
    {
        //Arrange 
        var parameter1 = Fixture.Create<string>();
        var parameter2 = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithStringGenerationResult(parameter1);
        _ = sut.CombineWithStringGenerationResult(parameter2);

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter1), 1)
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter2), 1);
    }
    
    [Fact]
    public void DidNotReceive_WhenMockedDependencyNotCalled_CanVerifyCorrectly()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.StringGenerationResult();

        //Assert
        AutoSubstitute
            .DidNotReceive<IReceivedHelperTestClassDependency>(x => x.CombinedWith(Arg.Any<string>()));
    }
    
    [Fact]
    public void DidNotReceive_WhenCalledWithRandomParameter_CanVerifyNoReceivedCallsForMockedDependency()
    {
        //Arrange 
        var parameter = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithStringGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .DidNotReceive<IReceivedHelperTestClassDependency>(x => x.CombinedWith(Fixture.Create<string>()));
    }
    
    private class ReceivedHelperTestClass
    {
        private readonly IReceivedHelperTestClassDependency _receivedHelperTestClassDependency;

        public ReceivedHelperTestClass(IReceivedHelperTestClassDependency receivedHelperTestClassDependency)
        {
            _receivedHelperTestClassDependency = receivedHelperTestClassDependency;
        }

        public string StringGenerationResult()
        {
            var value = _receivedHelperTestClassDependency.Generate();
            return value;
        }

        public string CombineWithStringGenerationResult(string combined)
        {
            var value = _receivedHelperTestClassDependency.CombinedWith(combined);
            return value;
        }
    }

    public interface IReceivedHelperTestClassDependency
    {
        string Generate();

        string CombinedWith(string parameterString);
    }
}