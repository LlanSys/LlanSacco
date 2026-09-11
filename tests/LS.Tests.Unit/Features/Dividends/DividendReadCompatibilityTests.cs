using LS.Application.Features.Dividends.Queries;
using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Dividends.DataContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LS.Tests.Unit.Features.Dividends;

public sealed class DividendReadCompatibilityTests
{
    [Fact]
    public async Task List_and_detail_agree_on_net_totals_and_empty_declarations()
    {
        var tenantId = Guid.CreateVersion7();
        var actorId = Guid.CreateVersion7().ToString();
        var tenant = Substitute.For<ICurrentTenantProvider>();
        tenant.TenantId.Returns(tenantId);
        await using var context = new DividendsDBContext(new DbContextOptionsBuilder<DividendsDBContext>()
            .UseInMemoryDatabase(Guid.CreateVersion7().ToString()).Options, tenant);
        var first = DividendDeclaration.Create(tenantId, 2026, 0.1m, 0.05m, createdBy: actorId);
        var second = DividendDeclaration.Create(tenantId, 2025, 0.1m, 0.05m, createdBy: actorId);
        var empty = DividendDeclaration.Create(tenantId, 2024, 0.1m, 0.05m, createdBy: actorId);
        context.AddRange(first, second, empty);
        context.AddRange(
            DividendCalculation.Create(tenantId, first.Id, Guid.CreateVersion7(), 1000, 100, 0, 0, 10, 0, actorId),
            DividendCalculation.Create(tenantId, first.Id, Guid.CreateVersion7(), 1000, 50, 0, 0, 5, 0, actorId),
            DividendCalculation.Create(tenantId, second.Id, Guid.CreateVersion7(), 1000, 500, 0, 0, 0, 0, actorId));
        await context.SaveChangesAsync(CancellationToken.None);
        context.ChangeTracker.Clear();
        var unitOfWork = Substitute.For<IDividendsUnitOfWork>();
        unitOfWork.DividendDeclarations.Returns(new Repository<DividendDeclaration>(context));
        unitOfWork.DividendCalculations.Returns(new Repository<DividendCalculation>(context));
        var services = new ServiceCollection();
        services.AddSingleton(unitOfWork);
        services.AddLogging();
        services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<GetDividendDeclarationsQuery>());
        await using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();
        var list = await sender.Send(new GetDividendDeclarationsQuery(), CancellationToken.None);
        Assert.True(list.IsSuccess);
        var rows = list.Data!.ToDictionary(d => d.Id);
        foreach (var (id, total) in new[] { (first.Id, 135m), (second.Id, 500m), (empty.Id, 0m) })
        {
            var detail = await sender.Send(new GetDividendDeclarationByIdQuery(id), CancellationToken.None);
            Assert.True(detail.IsSuccess);
            Assert.Equal(total, detail.Data!.TotalCalculatedAmount);
            Assert.Equal(total, rows[id].TotalCalculatedAmount);
            Assert.Equal(rows[id].FinancialYear, detail.Data.FinancialYear);
            Assert.Null(detail.Data.Notes);
        }
        Assert.Equal("2026", rows[first.Id].FinancialYear);
        var missing = await sender.Send(new GetDividendDeclarationByIdQuery(Guid.CreateVersion7()), CancellationToken.None);
        Assert.False(missing.IsSuccess);
        Assert.NotNull(missing.Error);
    }
}
