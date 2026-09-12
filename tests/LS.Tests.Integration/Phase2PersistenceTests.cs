using LS.Application.Features.Banking.Savings.Commands;
using LS.Application.Features.Banking.Deposits.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Savings.Enums;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Persistence.Features.Banking.DataContext;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using LS.SharedKernel.Features.Banking.Savings.Events;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using System.Data.Common;
using LS.Application.Features.CheckOff.Commands.Batches;
using LS.Application.Features.HR.Payroll.Commands;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Departments.Entities;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Domain.Features.HR.Payroll.Entities;
using LS.Domain.Features.HR.Payroll.Services;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.CheckOff.Contracts;
using LS.Persistence.Features.CheckOff.DataContext;
using LS.Persistence.Features.HR;
using LS.Persistence.Features.HR.DataContext;
using LS.Tests.Integration.TestFixtures;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Tests.Integration;

public abstract class Phase2PersistenceTests<TFixture>(TFixture fixture) : IClassFixture<TFixture> where TFixture : DbFixture
{
    private readonly string _databaseName = "phase2_" + Guid.CreateVersion7().ToString("N");
    private sealed class Tenant : ICurrentTenantProvider { public Guid TenantId { get; set; } = Guid.CreateVersion7(); }
    private sealed class Actor : ICurrentActorProvider { public string ActorId => "00000000-0000-0000-0000-000000000001"; }
    private sealed class Reads : DbCommandInterceptor
    {
        public int Count { get; set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) Count++;
            return ValueTask.FromResult(result);
        }
    }
    private DbContextOptions<TContext> Options<TContext>(Reads? reads = null) where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>();
        if (fixture is PostgreSqlDbFixture) options.UseNpgsql(new Npgsql.NpgsqlConnectionStringBuilder(fixture.GetConnectionString()) { Database = _databaseName }.ConnectionString);
        else options.UseSqlServer(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(fixture.GetConnectionString()) { InitialCatalog = _databaseName }.ConnectionString);
        if (reads is not null) options.AddInterceptors(reads);
        return options.Options;
    }
    private static ServiceProvider Services<TContext>(TContext context, Tenant tenant, bool failSavingsPublication = false) where TContext : DbContext
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(context);
        services.AddSingleton<ICurrentTenantProvider>(tenant);
        services.AddSingleton<ICurrentActorProvider>(new Actor());
        services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<RunPayrollCommand>());
        services.AddSingleton<IKenyaTaxCalculatorService, KenyaTaxCalculatorService>();
        if (context is HrDBContext)
        {
            // Resolve the real production implementations, including internal thin repositories.
            foreach (var contract in typeof(IHrUnitOfWork).GetProperties().Select(p => p.PropertyType))
            {
                var implementation = typeof(HrUnitOfWork).Assembly.GetTypes().Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
                services.AddScoped(contract, sp => ActivatorUtilities.CreateInstance(sp, implementation));
            }
            services.AddScoped<IHrUnitOfWork, HrUnitOfWork>();
        }
        else if (context is BankingDBContext)
        {
            var implementation = typeof(BankingDBContext).Assembly.GetTypes()
                .Single(t => t.IsClass && !t.IsAbstract && typeof(IBankingUnitOfWork).IsAssignableFrom(t));
            services.AddScoped(typeof(IBankingUnitOfWork), sp => ActivatorUtilities.CreateInstance(sp, implementation));
            if (failSavingsPublication)
                services.AddTransient<INotificationHandler<SavingsDepositedIntegrationEvent>, RejectSavingsPublication>();
        }
        else services.AddScoped<ICheckOffUnitOfWork, CheckOffUnitOfWork>();
        return services.BuildServiceProvider();
    }

    private sealed class RejectSavingsPublication : INotificationHandler<SavingsDepositedIntegrationEvent>
    {
        public Task Handle(SavingsDepositedIntegrationEvent notification, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Injected publication failure.");
    }

    [Fact]
    public async Task Savings_deposits_and_withdrawal_persist_balances_and_fee_from_fresh_contexts()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var memberId = Guid.CreateVersion7();
        var product = SavingsProduct.Create("Savings", "SAV", 0, 10, true, 5, null, null, true, new Actor().ActorId);
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Add(product);
            await setup.SaveChangesAsync();
        }
        // The second deposit must update a detached existing account, not just insert a transaction.
        foreach (var amount in new[] { 100m, 50m })
        {
            await using var context = new BankingDBContext(options, tenant, new Actor());
            await using var services = Services(context, tenant);
            var result = await services.GetRequiredService<ISender>().Send(new DepositSavingsCommand(
                new DepositSavingsRequest(memberId, product.Id, amount, null, null)));
            Assert.True(result.IsSuccess, result.Error?.Message);
        }
        await using (var context = new BankingDBContext(options, tenant, new Actor()))
        {
            Assert.Equal(150m, (await context.SavingsAccounts.AsNoTracking().SingleAsync()).Balance);
            await using var services = Services(context, tenant);
            var result = await services.GetRequiredService<ISender>().Send(new WithdrawSavingsCommand(
                new WithdrawSavingsRequest(memberId, product.Id, 40m, null, null)));
            Assert.True(result.IsSuccess, result.Error?.Message);
            Assert.Equal(105m, result.Data!.Balance);
        }
        await using (var fresh = new BankingDBContext(options, tenant, new Actor()))
        {
            var account = await fresh.SavingsAccounts.SingleAsync();
            Assert.Equal(105m, account.Balance);
            Assert.Equal(95m, account.GetAvailableBalance(product.MinimumBalance));
            var transactions = await fresh.SavingsTransactions.ToListAsync();
            Assert.Equal(4, transactions.Count);
            Assert.Equal(150m, transactions.Where(t => t.Type == SavingsTransactionType.Deposit).Sum(t => t.Amount));
            Assert.Equal(40m, Assert.Single(transactions, t => t.Type == SavingsTransactionType.Withdrawal).Amount);
            Assert.Equal(5m, Assert.Single(transactions, t => t.Type == SavingsTransactionType.Fee).Amount);
        }
    }

    [Fact]
    public async Task First_savings_deposit_does_not_commit_an_empty_account_when_publication_fails()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var product = SavingsProduct.Create("Savings", "SAV", 0, 0, true, 0, null, null, true, new Actor().ActorId);
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Add(product);
            await setup.SaveChangesAsync();
        }
        await using (var context = new BankingDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(context, tenant, failSavingsPublication: true);
            var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => services.GetRequiredService<ISender>()
                .Send(new DepositSavingsCommand(new DepositSavingsRequest(Guid.CreateVersion7(), product.Id, 100, null, null))));
            Assert.Equal("Injected publication failure.", failure.Message);
        }
        await using var fresh = new BankingDBContext(options, tenant, new Actor());
        Assert.False(await fresh.SavingsAccounts.AnyAsync());
        Assert.False(await fresh.SavingsTransactions.AnyAsync());
    }

    [Theory]
    [InlineData(100, false, 100)]
    [InlineData(110, true, 0)]
    public async Task Early_withdrawal_checks_penalty_before_staging_changes(decimal balance, bool succeeds, decimal expectedBalance)
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var product = new DepositProduct { Name = "Fixed", Code = "FIX", CreatedBy = new Actor().ActorId, PenaltyStrategy = EarlyWithdrawalPenaltyStrategy.FlatPercentage, FlatPenaltyRate = 10 };
        var account = DepositAccount.Create(Guid.CreateVersion7(), product.Id, DateTimeOffset.UtcNow.AddMonths(1), new Actor().ActorId);
        account.Balance = balance;
        account.AccruedInterest = 25;
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(product, account);
            await setup.SaveChangesAsync();
        }
        await using (var context = new BankingDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(context, tenant);
            var result = await services.GetRequiredService<ISender>().Send(new WithdrawFromDepositCommand(account.Id, new DepositTransactionRequest(100, "test")));
            Assert.Equal(succeeds, result.IsSuccess);
            // A later save on the same unit of work must not charge a rejected withdrawal's penalty.
            await services.GetRequiredService<IBankingUnitOfWork>().CompleteAsync(CancellationToken.None);
        }
        await using var fresh = new BankingDBContext(options, tenant, new Actor());
        var persisted = await fresh.DepositAccounts.SingleAsync();
        Assert.Equal(expectedBalance, persisted.Balance);
        Assert.Equal(25m, persisted.AccruedInterest);
        var transactions = await fresh.DepositTransactions.ToListAsync();
        Assert.Equal(succeeds ? 2 : 0, transactions.Count);
        if (succeeds)
        {
            Assert.Equal(10m, Assert.Single(transactions, t => t.Type == DepositTransactionType.Penalty).Amount);
            Assert.Equal(100m, Assert.Single(transactions, t => t.Type == DepositTransactionType.Withdrawal).Amount);
        }
    }

    [Fact]
    public async Task Payroll_uses_period_configuration_and_preserves_prior_run_from_a_fresh_context()
    {
        var tenant = new Tenant();
        var reads = new Reads();
        var options = Options<HrDBContext>(reads);
        Guid periodId;
        await using (var context = new HrDBContext(options, tenant, new Actor()))
        {
            await context.Database.EnsureCreatedAsync();
            var department = Department.Create("PAY", "Payroll", "", new Actor().ActorId);
            var employee = Employee.Create("E1", "e1@example.test", "One", "Employee", "123", "+254", "700000001", "+254700000001", department.Id, null, new Actor().ActorId);
            var period = PayrollPeriod.Create(2025, 1, new Actor().ActorId);
            periodId = period.Id;
            var effective = PayrollStatutoryConfiguration.Create(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), 0, 0, 0, 0, 0, 0, new Actor().ActorId);
            effective.AddOrUpdatePayeTaxBand(0, null, 10);
            var future = PayrollStatutoryConfiguration.Create(new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero), 0, 0, 0, 0, 0, 0, new Actor().ActorId);
            future.AddOrUpdatePayeTaxBand(0, null, 90);
            context.AddRange(department, employee, period, effective, future, EmployeeSalary.Create(employee.Id, 10000, new Actor().ActorId));
            await context.SaveChangesAsync();
        }
        await using (var context = new HrDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(context, tenant);
            reads.Count = 0;
            var result = await services.GetRequiredService<ISender>().Send(new RunPayrollCommand(periodId));
            Assert.True(result.IsSuccess, result.Error?.Message);
            Assert.InRange(reads.Count, 1, 9);
        }
        Guid payslipId;
        await using (var fresh = new HrDBContext(options, tenant, new Actor()))
        {
            var payslip = await fresh.Payslips.SingleAsync(p => p.PayrollPeriodId == periodId);
            Assert.Equal(1000m, payslip.PayeAmount);
            Assert.Equal(9000m, payslip.NetPay);
            payslipId = payslip.Id;
            Assert.NotNull((await fresh.PayrollPeriods.SingleAsync(p => p.Id == periodId)).ProcessedAt);
            await using var services = Services(fresh, tenant);
            Assert.False((await services.GetRequiredService<ISender>().Send(new RunPayrollCommand(periodId))).IsSuccess);
        }
        await using var verify = new HrDBContext(options, tenant, new Actor());
        Assert.Equal(payslipId, (await verify.Payslips.SingleAsync(p => p.PayrollPeriodId == periodId)).Id);
    }

    [Fact]
    public async Task Transaction_rollback_discards_staged_mutations_even_when_context_is_reused()
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        await using var context = new HrDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        await using var services = Services(context, tenant);
        var uow = services.GetRequiredService<IHrUnitOfWork>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.ExecuteInTransactionAsync<bool>(async () =>
        {
            await uow.PayrollPeriodRepository.CreateAsync(PayrollPeriod.Create(2025, 2, new Actor().ActorId));
            await context.SaveChangesAsync(); // Simulate a downstream failure after a database write inside the transaction.
            throw new InvalidOperationException("Injected failure after saving");
        }, CancellationToken.None));
        await uow.CompleteAsync();
        await using var fresh = new HrDBContext(options, tenant, new Actor());
        Assert.Empty(await fresh.PayrollPeriods.ToListAsync());
    }

    [Fact]
    public async Task Validation_persists_rows_batches_and_rejects_duplicate_payroll_numbers()
    {
        var tenant = new Tenant();
        var reads = new Reads();
        var options = Options<CheckOffDBContext>(reads);
        Guid validId;
        Guid invalidId;
        await using (var context = new CheckOffDBContext(options, tenant, new Actor()))
        {
            await context.Database.EnsureCreatedAsync();
            var employer = Employer.Create(tenant.TenantId, "Employer", "Contact", "employer@example.test", "+254700000000", new Actor().ActorId);
            var member = Member.Create(tenant.TenantId, "M1", "One", "Member", "m1@example.test", "+254700000001", "ID1", new DateOnly(1990, 1, 1), default, new Actor().ActorId);
            var valid = CheckoffBatch.Create(tenant.TenantId, employer.Id, "VALID", DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc), 200, new Actor().ActorId);
            var invalid = CheckoffBatch.Create(tenant.TenantId, employer.Id, "DUPLICATE", DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc), 20, new Actor().ActorId);
            validId = valid.Id;
            invalidId = invalid.Id;
            context.AddRange(employer, member, valid, invalid, MemberEmployment.Create(tenant.TenantId, member.Id, employer.Id, "E1", new Actor().ActorId));
            context.AddRange(CheckoffStagingRow.Create(tenant.TenantId, valid.Id, "E1", "One", 10, 0, 0, new Actor().ActorId),
                CheckoffStagingRow.Create(tenant.TenantId, invalid.Id, "E1", "One", 10, 0, 0, new Actor().ActorId),
                CheckoffStagingRow.Create(tenant.TenantId, invalid.Id, "E1", "One", 10, 0, 0, new Actor().ActorId));
            for (var number = 2; number <= 20; number++)
            {
                var payrollNumber = "E" + number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                context.Add(MemberEmployment.Create(tenant.TenantId, member.Id, employer.Id, payrollNumber, new Actor().ActorId));
                context.Add(CheckoffStagingRow.Create(tenant.TenantId, valid.Id, payrollNumber, "One", 10, 0, 0, new Actor().ActorId));
            }
            await context.SaveChangesAsync();
        }
        await using (var context = new CheckOffDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(context, tenant);
            var sender = services.GetRequiredService<ISender>();
            reads.Count = 0;
            Assert.True((await sender.Send(new ValidateCheckoffBatchCommand(validId))).Data);
            Assert.InRange(reads.Count, 1, 3);
            Assert.False((await sender.Send(new ValidateCheckoffBatchCommand(invalidId))).Data);
        }
        await using (var fresh = new CheckOffDBContext(options, tenant, new Actor()))
        {
            Assert.Equal(CheckoffBatchStatus.Validated, (await fresh.CheckoffBatches.SingleAsync(b => b.Id == validId)).Status);
            var validatedRows = await fresh.CheckoffStagingRows.Where(r => r.CheckoffBatchId == validId).ToListAsync();
            Assert.Equal(20, validatedRows.Count);
            Assert.All(validatedRows, row => Assert.Equal(CheckoffRowStatus.Validated, row.Status));
            Assert.Equal(CheckoffBatchStatus.Failed, (await fresh.CheckoffBatches.SingleAsync(b => b.Id == invalidId)).Status);
            Assert.All(await fresh.CheckoffStagingRows.Where(r => r.CheckoffBatchId == invalidId).ToListAsync(), r => { Assert.Null(r.ResolvedMemberId); Assert.Contains("Duplicate", r.ExceptionReason); });
        }
        await using var otherTenant = new CheckOffDBContext(options, new Tenant(), new Actor());
        Assert.Empty(await otherTenant.CheckoffBatches.ToListAsync());
        Assert.Empty(await otherTenant.CheckoffStagingRows.ToListAsync());
    }
    [Fact]
    public async Task Two_payroll_runs_cannot_both_commit_the_same_period()
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        Guid periodId;
        await using (var setup = new HrDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            var period = PayrollPeriod.Create(2025, 3, new Actor().ActorId);
            setup.Add(period);
            periodId = period.Id;
            await setup.SaveChangesAsync();
        }
        await using var first = new HrDBContext(options, tenant, new Actor());
        await using var second = new HrDBContext(options, tenant, new Actor());
        var firstPeriod = await first.PayrollPeriods.SingleAsync(p => p.Id == periodId);
        var secondPeriod = await second.PayrollPeriods.SingleAsync(p => p.Id == periodId);
        firstPeriod.RecordPayrollRun(new Actor().ActorId);
        secondPeriod.RecordPayrollRun(new Actor().ActorId);
        await first.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }
}
