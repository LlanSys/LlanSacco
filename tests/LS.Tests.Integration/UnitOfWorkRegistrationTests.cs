using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.Accounting.Extensions;
using LS.Persistence.Features.Banking.Extensions;
using LS.Persistence.Features.CheckOff.Extensions;
using LS.Persistence.Features.ControlPlane.Extensions;
using LS.Persistence.Features.Dividends.Extensions;
using LS.Persistence.Features.HR.Extensions;
using LS.Persistence.Features.IAM.Extensions;
using LS.Persistence.Features.Loans.Extensions;
using LS.Persistence.Features.Membership.Extensions;
using LS.Persistence.Features.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace LS.Tests.Integration;

public sealed class UnitOfWorkRegistrationTests
{
    [Theory]
    [InlineData("PostgreSql", "Host=localhost;Database=RepositoryRegistration;Username=test")]
    public async Task Repositories_resolve_from_the_owning_unit_of_work_scope(string databaseProvider, string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString
        })
            .Build();

        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var tenant = Substitute.For<ICurrentTenantProvider>();
        tenant.TenantId.Returns(Guid.CreateVersion7());
        var actor = Substitute.For<ICurrentActorProvider>();
        actor.ActorId.Returns(ICurrentActorProvider.SystemActor);

        var services = new ServiceCollection()
            .AddSingleton(tenant)
            .AddSingleton(actor)
            .AddLogging()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IHrUnitOfWork>());

        services.AddAccountingPersistence(configuration);
        services.AddBankingPersistence(configuration, isProduction: false);
        services.AddCheckOffPersistence(configuration);
        services.AddControlPlanePersistence(configuration, environment);
        services.AddDividendsPersistence(configuration);
        services.AddHrPersistence(configuration);
        services.AddIamPersistence(configuration, environment);
        services.AddLoansPersistence(configuration);
        services.AddMembershipPersistence(configuration);
        services.AddSharedPersistence(configuration, environment);

        Type[] contracts =
        [
            typeof(IAccountingUnitOfWork), typeof(IBankingUnitOfWork), typeof(ICheckOffUnitOfWork),
            typeof(IControlPlaneUnitOfWork), typeof(IDividendsUnitOfWork), typeof(IHrUnitOfWork),
            typeof(IIamUnitOfWork), typeof(ILoansUnitOfWork), typeof(IMembershipUnitOfWork), typeof(ISharedUnitOfWork)
        ];

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var first = provider.CreateAsyncScope();
        await using var next = provider.CreateAsyncScope();

        foreach (var contract in contracts)
        {
            var unit = first.ServiceProvider.GetRequiredService(contract);
            var otherUnit = next.ServiceProvider.GetRequiredService(contract);

            foreach (var property in contract.GetProperties())
            {
                var repository = property.GetValue(unit);
                Assert.Same(first.ServiceProvider.GetRequiredService(property.PropertyType), repository);
                Assert.NotSame(repository, property.GetValue(otherUnit));
            }
        }
    }
}
