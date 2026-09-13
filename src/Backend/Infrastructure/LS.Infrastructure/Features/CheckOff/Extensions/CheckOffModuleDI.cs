using LS.Persistence.Features.CheckOff.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.CheckOff.Extensions;

public static class CheckOffModuleDI
{
    public static IServiceCollection AddCheckOffModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCheckOffPersistence(configuration);
        return services;
    }
}
