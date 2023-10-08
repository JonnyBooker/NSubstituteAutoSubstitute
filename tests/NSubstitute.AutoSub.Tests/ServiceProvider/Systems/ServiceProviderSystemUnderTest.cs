using Microsoft.Extensions.DependencyInjection;
using NSubstitute.AutoSub.Tests.ServiceProvider.Dependencies;

namespace NSubstitute.AutoSub.Tests.ServiceProvider.Systems;

public class ServiceProviderSystemUnderTest
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderSystemUnderTest(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string? Generate()
    {
        var textGenerationDependency = _serviceProvider.GetService<IServiceProviderTextGenerationDependency>();
        return textGenerationDependency?.Generate();
    }

    public void GenerateUsingNotRegisteredDependency()
    {
        _ = _serviceProvider.GetRequiredService<INotRegisteredServiceDependency>();
    }
}