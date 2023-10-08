using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Tests.PartsOf.Systems;
using Xunit;

namespace NSubstitute.AutoSub.Tests.PartsOf;

public class SimplePartsOfSystemUnderTestManualWithExceptionsTests
{
    private AutoSubstitute AutoSubstitute { get; } = new(SubstituteBehaviour.ManualWithExceptions);

    [Fact]
    public void SimplePartsOfSystemUnderTest_WhenUsedWithManualWithExceptions_WillThrowExceptionAndCreateDiagnosticLog()
    {
        //Arrange
        var sutType = typeof(SimplePartsOfSystemUnderTest);
        var dependencyType = typeof(SimplePartsOfPartsOfDependency);
        
        var messages = new List<(Type Type, string Message)>();
        
        AutoSubstitute.DiagnosticsHandler.DiagnosticLogAdded += (_, args) =>
        {
            messages.Add((args.Type, args.Message));
        };

        //Act & Assert
        Assert.Throws<AutoSubstituteException>(() =>
        {
            _ = AutoSubstitute.CreateInstance<SimplePartsOfSystemUnderTest>();
        });

        Assert.Contains(messages, x => x.Type == sutType && x.Message == "No suitable constructor found for type");
        Assert.Contains(messages, x => x.Type == dependencyType && x.Message == "When using behaviour 'Manual with Exceptions', only interfaces are usable with this behaviour");
    }
}