using AutoFixture;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Tests.Behaviour.Dependencies;
using NSubstitute.AutoSub.Tests.Behaviour.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.Behaviour;

public class ManualTests
{
    [Fact]
    public void SubstituteBehaviourManual_WhenUsedAndNoDependencyMockedPriorToCreate_WillThrowAutoSubstituteException()
    {
        //Arrange
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.Manual);
        
        //Act & Assert
        var exception = Assert.Throws<AutoSubstituteException>(() =>
        {
            _ = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();
        });
        
        Assert.Equal("Unable to find suitable constructor. You are using 'Manual' behaviour mode, a mock must be created for the method before the instance is created. Alternatively, use an 'Automatic' behaviour mode", 
            exception.Message);
    }
}