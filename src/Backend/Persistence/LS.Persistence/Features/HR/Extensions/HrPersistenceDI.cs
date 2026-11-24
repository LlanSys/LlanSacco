using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Departments.Contracts.Repositories;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.HR.Payroll.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Features.HR.DataContext;
using LS.Persistence.Features.HR.Departments.Repositories;
using LS.Persistence.Features.HR.Employees.Repositories;
using LS.Persistence.Features.HR.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using LS.Persistence.Common.Interceptors;

namespace LS.Persistence.Features.HR.Extensions;

public static class HrPersistenceDI
{
    public static IServiceCollection AddHrPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HrConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("HrConnection (or DefaultConnection) not found.");


        services.TryAddScoped<TenantConnectionInterceptor>();

        void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                connectionString,
                pgOptions =>
                {
                    pgOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    pgOptions.CommandTimeout(30);
                    pgOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_HR");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<HrDBContext, HrPostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<IDepartmentRepository, HrDepartmentRepository>();
        services.AddScoped<IEmployeeRepository, HrEmployeeRepository>();
        services.AddScoped<IEmployeeNumberSequenceRepository, EmployeeNumberSequenceRepository>();

        services.AddScoped<IPayrollStatutoryConfigurationRepository, PayrollStatutoryConfigurationRepository>();
        services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();
        services.AddScoped<IEmployeeSalaryRepository, EmployeeSalaryRepository>();
        services.AddScoped<IPayrollComponentRepository, PayrollComponentRepository>();
        services.AddScoped<IEmployeePayrollComponentRepository, EmployeePayrollComponentRepository>();
        services.AddScoped<IPayslipRepository, PayslipRepository>();

        services.AddScoped<IHrUnitOfWork, HrUnitOfWork>();

        return services;
    }
}


