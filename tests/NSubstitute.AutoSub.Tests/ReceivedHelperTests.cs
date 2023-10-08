using AutoFixture;
using NSubstitute.ReceivedExtensions;
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
        _ = sut.Generate();

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
        _ = sut.CombineWithTextGenerationResult(parameter);

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
        _ = sut.CombineWithTextGenerationResult(parameter1);
        _ = sut.CombineWithTextGenerationResult(parameter2);

        //Assert
        AutoSubstitute
            .ReceivedOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter1))
            .ReceivedOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter2));
    }
    
    [Fact]
    public void ReceivedAtLeastOnce_WhenCalledWithNoParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.Generate();

        //Assert
        AutoSubstitute
            .ReceivedAtLeastOnce<IReceivedHelperTestClassDependency>(x => x.Generate());
    }
    
    [Fact]
    public void ReceivedAtLeastOnce_WhenCalledWithParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange 
        var parameter = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithTextGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .ReceivedAtLeastOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter));
    }
    
    [Fact]
    public void ReceivedAtLeastOnce_WhenCalledMultipleTimes_CanChainVerify()
    {
        //Arrange 
        var parameter1 = Fixture.Create<string>();
        var parameter2 = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithTextGenerationResult(parameter1);
        _ = sut.CombineWithTextGenerationResult(parameter2);

        //Assert
        AutoSubstitute
            .ReceivedAtLeastOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter1))
            .ReceivedAtLeastOnce<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter2));
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledWithNoParameters_CanVerifyReceivedCallsForMockedDependency()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.Generate();

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
        _ = sut.CombineWithTextGenerationResult(parameter);

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
        _ = sut.CombineWithTextGenerationResult(parameter1);
        _ = sut.CombineWithTextGenerationResult(parameter2);

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter1), 1)
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter2), 1);
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledWithNoParameters_CanVerifyReceivedQuantityCallsForMockedDependency()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.Generate();

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.Generate(), Quantity.Exactly(1));
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledWithParameters_CanVerifyReceivedQuantityCallsForMockedDependency()
    {
        //Arrange 
        var parameter = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithTextGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter), Quantity.Exactly(1));
    }
    
    [Fact]
    public void ReceivedTimes_WhenCalledMultipleTimes_CanChainVerifyQuantity()
    {
        //Arrange 
        var parameter1 = Fixture.Create<string>();
        var parameter2 = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithTextGenerationResult(parameter1);
        _ = sut.CombineWithTextGenerationResult(parameter2);

        //Assert
        AutoSubstitute
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter1), Quantity.Exactly(1))
            .ReceivedTimes<IReceivedHelperTestClassDependency>(x => x.CombinedWith(parameter2), Quantity.Exactly(1));
    }
    
    [Fact]
    public void DidNotReceive_WhenMockedDependencyNotCalled_CanVerifyCorrectly()
    {
        //Arrange & Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.Generate();

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
        _ = sut.CombineWithTextGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .DidNotReceive<IReceivedHelperTestClassDependency>(x => x.CombinedWith(Fixture.Create<string>()));
    }
    
    [Fact]
    public void DidNotReceive_WhenCalledMultipleTimes_CanChainVerify()
    {
        //Arrange 
        var parameter = Fixture.Create<string>();
        
        //Act
        var sut = AutoSubstitute.CreateInstance<ReceivedHelperTestClass>();
        _ = sut.CombineWithTextGenerationResult(parameter);

        //Assert
        AutoSubstitute
            .DidNotReceive<IReceivedHelperTestClassDependency>(x => x.CombinedWith(Fixture.Create<string>()))
            .DidNotReceive<IReceivedHelperTestClassDependency>(x => x.CombinedWith(Fixture.Create<string>()));
    }
    
    private class ReceivedHelperTestClass
    {
        private readonly IReceivedHelperTestClassDependency _receivedHelperTestClassDependency;

        public ReceivedHelperTestClass(IReceivedHelperTestClassDependency receivedHelperTestClassDependency)
        {
            _receivedHelperTestClassDependency = receivedHelperTestClassDependency;
        }

        public string Generate()
        {
            var value = _receivedHelperTestClassDependency.Generate();
            return value;
        }

        public string CombineWithTextGenerationResult(string combined)
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