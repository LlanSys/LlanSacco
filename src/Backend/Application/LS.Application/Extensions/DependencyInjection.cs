using LS.Application.Behaviours;
using LS.Application.Configuration;
using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Application.Features.Accounting.Services;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Contracts.Implementations.Common;
using LS.Application.Features.HR.Employees.Contracts.Implementations;
using LS.Application.Features.HR.Employees.Contracts.Interfaces;
using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Application.Features.Membership.Contracts.Implementations;
using LS.Domain.Features.HR.Payroll.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LS.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        RegisterApplicationServices(services, configuration);
        return services;
    }

    private static void RegisterApplicationServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<MediatRSettings>(configuration.GetSection(MediatRSettings.SectionName));

        var mediatrSettings = configuration.GetSection(MediatRSettings.SectionName).Get<MediatRSettings>() ?? new MediatRSettings();
        var licenseKey = mediatrSettings.LicenseKey;
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            licenseKey = "your-community-license-key-here";
        }

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.LicenseKey = licenseKey;
            cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));

        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


        services.AddScoped<IEmployeeNumberGenerator, EmployeeNumberGenerator>();
        services.AddScoped<IMemberNumberGenerator, MemberNumberGenerator>();
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<IKenyaTaxCalculatorService, KenyaTaxCalculatorService>();

    }
}
