using AutoFixture;
using NSubstitute.AutoSub.Tests.Behaviour.Dependencies;
using NSubstitute.AutoSub.Tests.Behaviour.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.Behaviour;

public class BehaviourTests
{
    private Fixture Fixture { get; } = new();

    [Fact]
    public void SubstituteBehaviourStrict_WhenUsed_WillHaveNullDependencyIfNotSpecified()
    {
        //Arrange
        var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.Strict);
        
        //Act
        var instance = autoSubstitute.CreateInstance<BehaviourSystemUnderTest>();

        Assert.Throws<NullReferenceException>(() =>
        {
            instance.StringGenerationResult();
        });
    }

    [Fact]
    public void Test()
    {
        var sub1 = Substitute.For<IBehaviourStringGenerationDependency>();
        var sub2 = Substitute.For<IBehaviourIntGenerationDependency>();

        var sut = new BehaviourSystemUnderTest(sub1, sub2);

        var result = sut.StringGenerationResult();
        
        Assert.Null(result);
    }
}