using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LS.Persistence.Features.Accounting.Extensions;

namespace LS.Infrastructure.Features.Accounting.Extensions;

public static class AccountingModuleDI
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountingPersistence(configuration);
        return services;
    }
}
