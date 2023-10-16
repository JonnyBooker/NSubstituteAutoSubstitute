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
    public void SimpleSystemUnderTest_WhenCreatedDuringAct_WillReturnExpectedMockedResult()
    {
        //Arrange
        const string expectedMessage = $"Substitute type 'IMixedSubstituteDependency' has been created using mixed means. Only one of these (e.g. SubstituteFor/SubstituteForPartsOf/Use/UseCollection) should be used when testing to avoid confusion of which should be used.";
        

        //Act & Assert
        _ = AutoSubstitute.SubstituteFor<IMixedSubstituteDependency>();
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            _ = AutoSubstitute.SubstituteForPartsOf<IMixedSubstituteDependency>();
        });

        Assert.Equal(expectedMessage, exception.Message);
    }

    public interface IMixedSubstituteDependency
    {
        //Nothing needed here
    }
}