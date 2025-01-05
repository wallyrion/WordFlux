using Microsoft.Extensions.DependencyInjection;

namespace Wordflux.Tests.Integration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RemoveAllImplementedBy(this IServiceCollection serviceCollection, string name)
    {
        var serviceDescriptor = serviceCollection
            .Where(descriptor => descriptor.ImplementationType != null && descriptor.ImplementationType.Name.Contains(name)).ToList();

        foreach (var v in serviceDescriptor)
        {
            serviceCollection.Remove(v);
        }

        return serviceCollection;
    }

    public static IServiceCollection RemoveAllImplementedBy<T>(this IServiceCollection serviceCollection)
    {
        var serviceDescriptor = serviceCollection
            .Where(descriptor => descriptor.ImplementationType == typeof(T)).ToList();

        foreach (var v in serviceDescriptor)
        {
            serviceCollection.Remove(v);
        }

        return serviceCollection;
    }
}
