using LS.Persistence.Features.Loans.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.Loans.Extensions;

public static class LoansModuleDI
{
    public static IServiceCollection AddLoansModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLoansPersistence(configuration);
        return services;
    }
}
