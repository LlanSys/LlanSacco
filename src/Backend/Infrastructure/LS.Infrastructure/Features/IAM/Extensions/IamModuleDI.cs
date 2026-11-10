using LS.Domain.Features.IAM.Users.Entities;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;
using LS.Infrastructure.Features.Shared.Notifications.Contracts.Implementations.Services;
using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Domain.Features.IAM.Contracts;
using LS.Infrastructure.Extensions;
using LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;
using LS.Infrastructure.Features.IAM.Users.Seeding;
using LS.Persistence.Features.IAM.DataContext;
using LS.Persistence.Features.IAM.Extensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.IAM.Extensions;

public static class IamModuleDI
{
    public static IServiceCollection AddIamModule(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddIamPersistence(configuration, environment);

        services.Configure<DevelopmentSeedSettings>(configuration.GetSection(DevelopmentSeedSettings.SectionName));

        services.AddIdentity<AppUser, AppRole>(DependencyInjection.ConfigureIdentityOptions)
            .AddEntityFrameworkStores<IamDBContext>()
            .AddDefaultTokenProviders();

        DependencyInjection.ConfigureAuthentication(services, configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Login>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<IAppUserService, AppUserService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ISmsComposer, SmsComposer>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IIdentityResolutionService, IdentityResolutionService>();
        services.AddScoped<DevelopmentIdentitySeeder>();

        return services;
    }
}
