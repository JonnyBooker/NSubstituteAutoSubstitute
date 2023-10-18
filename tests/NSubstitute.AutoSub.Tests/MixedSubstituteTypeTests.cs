using AutoFixture;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Tests.For.Dependencies;
using NSubstitute.AutoSub.Tests.For.Implementations;
using NSubstitute.AutoSub.Tests.For.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests;

public class MixedSubstituteTypeTests
{
    private AutoSubstitute AutoSubstitute { get; } = new();

    [Fact]
    public void SimpleSystemUnderTest_WhenMixingForAndForPartsOf_WillThrowException()
    {
        //Arrange
        const string expectedMessage = $"Substitute type 'MixedSubstituteDependency' has been created using mixed methods. Only one of these (e.g. GetSubstituteFor/GetSubstituteForPartsOf/Use/UseCollection) should be used when testing to avoid confusion of which should be used.";
        
        //Act & Assert
        _ = AutoSubstitute.GetSubstituteFor<MixedSubstituteDependency>();
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            _ = AutoSubstitute.GetSubstituteForPartsOf<MixedSubstituteDependency>();
        });

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenMixingForPartsOfAndFor_WillThrowException()
    {
        //Arrange
        const string expectedMessage = $"Substitute type 'MixedSubstituteDependency' has been created using mixed methods. Only one of these (e.g. GetSubstituteFor/GetSubstituteForPartsOf/Use/UseCollection) should be used when testing to avoid confusion of which should be used.";
        
        //Act & Assert
        _ = AutoSubstitute.GetSubstituteForPartsOf<MixedSubstituteDependency>();
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            _ = AutoSubstitute.GetSubstituteFor<MixedSubstituteDependency>();
        });

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void SimpleSystemUnderTest_WhenUsingUse_WillGetBackManuallySuppliedObject()
    {
        //Arrange
        var mixedSubstituteDependency = Substitute.For<MixedSubstituteDependency>();
        AutoSubstitute.Use(mixedSubstituteDependency);
        
        //Act 
        var retrievedDependency = AutoSubstitute.GetSubstituteFor<MixedSubstituteDependency>();
        
        //Assert
        Assert.Equal(mixedSubstituteDependency, retrievedDependency);
    }

    public class MixedSubstituteDependency
    {
        //Nothing needed here
    }
}