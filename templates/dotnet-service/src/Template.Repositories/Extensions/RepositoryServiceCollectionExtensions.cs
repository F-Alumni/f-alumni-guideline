using Microsoft.Extensions.DependencyInjection;
using Template.Repositories.Implementations;
using Template.Repositories.Interfaces;

namespace Template.Repositories.Extensions;

public static class RepositoryServiceCollectionExtensions
{
    /// <summary>
    /// Registers repository layer services and data access dependencies.
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ISampleItemRepository, SampleItemRepository>();
        return services;
    }
}
