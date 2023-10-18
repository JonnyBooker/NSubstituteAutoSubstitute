using AutoFixture;
using NSubstitute.AutoSub.Tests.ServiceProvider.Dependencies;
using NSubstitute.AutoSub.Tests.ServiceProvider.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.ServiceProvider;

public class ServiceProviderTests
{
    private Fixture Fixture { get; } = new();
    
    private AutoSubstitute AutoSubstitute { get; } = new();
    
    [Fact]
    public void AutoSubstitute_WhenUtilisedAsServiceProviderForRegisteredSubstituteUsingGetService_WillReturnAsServiceProvider()
    {
        //Arrange
        var expectedString = Fixture.Create<string>();

        AutoSubstitute
            .GetSubstituteFor<IServiceProviderTextGenerationDependency>()
            .Generate()
            .Returns(expectedString);
        
        //Act
        var sut = new ServiceProviderSystemUnderTest(AutoSubstitute);
        var result = sut.Generate();
        
        //Assert
        Assert.Equal(expectedString, result);
    }
    
    [Fact]
    public void AutoSubstitute_WhenUtilisedAsServiceProviderForRegisteredSubstituteUsingGetRequiredService_WillReturnNull()
    {
        //Arrange & Act
        var sut = new ServiceProviderSystemUnderTest(AutoSubstitute);
        var result = sut.Generate();
        
        //Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void AutoSubstitute_WhenUtilisedAsServiceProviderForNonRegisteredSubstitute_WillThrowError()
    {
        //Arrange & Act
        var sut = new ServiceProviderSystemUnderTest(AutoSubstitute);
        
        //Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            sut.GenerateUsingNotRegisteredDependency();
        });
        
        Assert.Equal("No service for type 'NSubstitute.AutoSub.Tests.ServiceProvider.Dependencies.INotRegisteredServiceDependency' has been registered.", exception.Message);
    }
}