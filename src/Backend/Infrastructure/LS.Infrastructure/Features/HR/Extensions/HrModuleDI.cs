using LS.Application.Features.HR.Employees.IntegrationEvents;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Infrastructure.Features.HR.Employees.EmailComposers;
using LS.Persistence.Features.HR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.HR.Extensions;

public static class HrModuleDI
{
    public static IServiceCollection AddHrModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHrPersistence(configuration);
        services.AddScoped<IEmailComposer<EmployeeCreatedIntegrationEvent>, EmployeeWelcomeEmailComposer>();
        return services;
    }
}
