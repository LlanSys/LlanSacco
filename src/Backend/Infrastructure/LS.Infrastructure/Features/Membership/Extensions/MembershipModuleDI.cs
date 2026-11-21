using LS.Application.Features.Membership.Contracts;
using LS.Infrastructure.Features.Membership.Contracts.Implementations.Services;
using LS.Persistence.Features.Membership.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.Membership.Extensions;

public static class MembershipModuleDI
{
    public static IServiceCollection AddMembershipModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMembershipPersistence(configuration);
        services.AddScoped<IKycOrchestrator, SimulatedKycOrchestrator>();
        return services;
    }
}
