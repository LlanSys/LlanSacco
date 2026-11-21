using LS.Persistence.Features.Dividends.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.Dividends.Extensions;

public static class DividendsModuleDI
{
    public static IServiceCollection AddDividendsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDividendsPersistence(configuration);
        
        return services;
    }
}
