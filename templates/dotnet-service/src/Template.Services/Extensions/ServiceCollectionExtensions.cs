using Microsoft.Extensions.DependencyInjection;
using Template.Repositories.Extensions;
using Template.Services.Implementations;
using Template.Services.Interfaces;

namespace Template.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers both Service and Repository layers into the dependency injection container.
    /// This allows the API layer to reference only the Services layer without directly referencing Repositories.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. Register lower-level Repository layer dependencies
        services.AddRepositories();

        // 2. Register Service layer dependencies
        services.AddScoped<ISampleItemService, SampleItemService>();

        return services;
    }
}
