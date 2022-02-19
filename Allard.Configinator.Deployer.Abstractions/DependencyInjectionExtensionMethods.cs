using Allard.Configinator.Deployer.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensionMethods
{
    /// <summary>
    ///     Register a single deployer.
    ///     This registers an IDeployerFactory of type SingleDeployerFactory,
    ///     and the IDeployer that it will provide.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddSingleDeployer<T>(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IDeployer
    {
        services.AddTransient<IDeployerFactory, SingleDeployerFactory>();
        services.Add(new ServiceDescriptor(typeof(IDeployer), typeof(T), lifetime));
        return services;
    }
}