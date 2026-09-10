using LS.Persistence.Features.Banking.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.Banking.Extensions;

public static class BankingModuleDI
{
    public static IServiceCollection AddBankingModule(this IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddBankingPersistence(configuration, isProduction);
        
        return services;
    }
}
