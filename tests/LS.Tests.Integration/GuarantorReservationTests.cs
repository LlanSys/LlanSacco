using FluentValidation;
using System.Text.Json;
using LS.Application.Features.Loans.Guarantors.Commands;
using LS.Application.Features.Loans.LoanApplications.Commands;
using LS.Application.Features.Loans.LoanRepayments.Commands;
using LS.Application.Features.Loans.Services;
using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Persistence.Features.Loans;
using LS.Persistence.Features.Loans.DataContext;
using LS.Application.Behaviours;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Banking.GuarantorReservations.Commands;
using LS.Application.Features.Banking.Savings.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using LS.Domain.Features.Banking.GuarantorReservations.Enums;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.Persistence.Features.Banking.DataContext;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Common.Enums;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using LS.Tests.Integration.TestFixtures;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LS.Infrastructure.Messaging.Consumers.Banking;
using LS.Infrastructure.Messaging.Consumers.Loans;
using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Application.Features.Accounting.Services;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Persistence.Features.Accounting;
using LS.Persistence.Features.Accounting.DataContext;
using LS.Infrastructure.Messaging.Consumers.Accounting;
using NSubstitute;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LS.Persistence.Features.Shared.Migrations.Generators;

namespace LS.Tests.Integration;

public abstract class GuarantorReservationTests<TFixture>(TFixture fixture) : IClassFixture<TFixture> where TFixture : DbFixture
{
    private readonly string _database = "guarantors_" + Guid.CreateVersion7().ToString("N");
    private readonly Tenant _tenant = new(Guid.CreateVersion7());
    private readonly Clock _clock = new();
    private readonly Actor _actor = new();
    private readonly MemberResolver _memberResolver = new();
    private int _productSequence;
    private const string ActorId = "00000000-0000-0000-0000-000000000001";

    private sealed record Tenant(Guid TenantId) : ICurrentTenantProvider;

    private sealed class Actor : ICurrentActorProvider
    {
        public string ActorId { get; set; } = GuarantorReservationTests<TFixture>.ActorId;
    }

    private sealed class MemberResolver : ICurrentMemberResolver
    {
        public Guid? MemberId { get; set; }
        public bool Verified { get; set; } = true;
        public Task<Guid?> FindActiveMemberIdAsync(CancellationToken cancellationToken) => Task.FromResult(MemberId);
        public Task<Guid?> FindVerifiedActiveMemberIdAsync(CancellationToken cancellationToken) => Task.FromResult(Verified ? MemberId : null);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 24, 10, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private DbContextOptions<BankingDBContext> Options(IInterceptor? interceptor = null)
    {
        var builder = new DbContextOptionsBuilder<BankingDBContext>();

        if (fixture is PostgreSqlDbFixture)
        {
            builder.UseNpgsql(new Npgsql.NpgsqlConnectionStringBuilder(fixture.GetConnectionString()) { Database = _database }.ConnectionString);
        }
        else
        {
            builder.UseSqlServer(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(fixture.GetConnectionString()) { InitialCatalog = _database }.ConnectionString);
        }

        if (interceptor is not null)
        {
            builder.AddInterceptors(interceptor);
        }

        return builder.Options;
    }

    private ServiceProvider Services(IInterceptor? interceptor = null, Tenant? tenant = null)
    {
        var services = new ServiceCollection().AddLogging();
        ConfigureServices(services, interceptor, tenant);
        return services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services, IInterceptor? interceptor = null, Tenant? tenant = null, string? brokerPrefix = null)
    {
        services.AddScoped<BackgroundExecutionContext>();
        services.AddSingleton<ICurrentTenantProvider>(tenant ?? _tenant);
        services.AddSingleton<ICurrentActorProvider>(_actor);
        services.AddSingleton<ICurrentMemberResolver>(_memberResolver);
        services.AddSingleton<TimeProvider>(_clock);
        services.AddScoped(sp => new BankingDBContext(Options(interceptor), sp.GetRequiredService<ICurrentTenantProvider>(), _actor));
        services.AddScoped(sp => new LoansDBContext(LoanOptions(), sp.GetRequiredService<ICurrentTenantProvider>(), _actor));
        services.AddScoped(sp => new AccountingDBContext(AccountingOptions(), sp.GetRequiredService<ICurrentTenantProvider>(), _actor));
        services.AddScoped<IAccountingUnitOfWork, AccountingUnitOfWork>();
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<GuarantorRecoveryCompletedIntegrationEventHandler>();
        services.AddTransient<IAmortizationStrategy, ReducingBalanceAmortizationStrategy>();
        services.AddTransient<IAmortizationStrategy, StraightLineAmortizationStrategy>();
        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblyContaining<ProcessGuarantorReservationCommand>();
            c.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<ProcessGuarantorReservationCommand>(includeInternalTypes: true);
        var implementations = typeof(BankingDBContext).Assembly.GetTypes();

        foreach (var property in new[] { typeof(IBankingUnitOfWork), typeof(ILoansUnitOfWork), typeof(IAccountingUnitOfWork) }.SelectMany(x => x.GetProperties()))
        {
            var contract = property.PropertyType;
            var implementation = implementations.Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
            services.AddScoped(contract, sp => ActivatorUtilities.CreateInstance(sp, implementation));
        }
        var unit = implementations.Single(t => t.IsClass && !t.IsAbstract && typeof(IBankingUnitOfWork).IsAssignableFrom(t));
        services.AddScoped(typeof(IBankingUnitOfWork), sp => ActivatorUtilities.CreateInstance(sp, unit));
        services.AddScoped<ILoansUnitOfWork, LoansUnitOfWork>();
        services.AddScoped<IContextEventPublisher<IBankingUnitOfWork>, ContextEventPublisher<IBankingUnitOfWork, BankingDBContext>>();
        services.AddScoped<IContextEventPublisher<ILoansUnitOfWork>, ContextEventPublisher<ILoansUnitOfWork, LoansDBContext>>();
        services.AddMassTransit(bus =>
        {
            if (brokerPrefix is not null)
            {
                bus.AddConsumer<GuarantorReservationInstructionConsumer>();
                bus.AddConsumer<GuarantorReservationOutcomeConsumer>();
            }
            bus.AddEntityFrameworkOutbox<BankingDBContext>(outbox =>
            {
                if (fixture is PostgreSqlDbFixture)
                {
                    outbox.UsePostgres(false);
                }
                else
                {
                    outbox.UseSqlServer(false);
                }

                outbox.UseBusOutbox();
            });

            if (brokerPrefix is null)
            {
                bus.UsingInMemory();
            }
            else
            {
                bus.UsingRabbitMq((context, rabbit) =>
                {
                    rabbit.Host(
                        Environment.GetEnvironmentVariable("BT_RABBITMQ_HOST")!,
                        ushort.Parse(Environment.GetEnvironmentVariable("BT_RABBITMQ_PORT")!, System.Globalization.CultureInfo.InvariantCulture),
                        Environment.GetEnvironmentVariable("BT_RABBITMQ_VIRTUAL_HOST")!,
                        host =>
                        {
                            host.Username(Environment.GetEnvironmentVariable("BT_RABBITMQ_USERNAME")!);
                            host.Password(Environment.GetEnvironmentVariable("BT_RABBITMQ_PASSWORD")!);
                        });

                    rabbit.Message<GuarantorReservationInstruction>(message => message.SetEntityName(brokerPrefix + "-instructions"));
                    rabbit.Message<GuarantorReservationOutcome>(message => message.SetEntityName(brokerPrefix + "-outcomes"));
                    rabbit.ReceiveEndpoint(
                        brokerPrefix + "-banking",
                        endpoint =>
                        {
                            endpoint.Durable = false;
                            endpoint.AutoDelete = true;
                            endpoint.ConfigureConsumer<GuarantorReservationInstructionConsumer>(context);
                        });

                    rabbit.ReceiveEndpoint(
                        brokerPrefix + "-loans",
                        endpoint =>
                        {
                            endpoint.Durable = false;
                            endpoint.AutoDelete = true;
                            endpoint.ConfigureConsumer<GuarantorReservationOutcomeConsumer>(context);
                        });
                });
            }

            bus.AddEntityFrameworkOutbox<LoansDBContext>(outbox =>
            {
                if (fixture is PostgreSqlDbFixture)
                {
                    outbox.UsePostgres(false);
                }
                else
                {
                    outbox.UseSqlServer(false);
                }

                outbox.UseBusOutbox();
            });
        });
    }

    private DbContextOptions<LoansDBContext> LoanOptions()
    {
        var builder = new DbContextOptionsBuilder<LoansDBContext>();

        if (fixture is PostgreSqlDbFixture)
        {
            builder.UseNpgsql(new Npgsql.NpgsqlConnectionStringBuilder(fixture.GetConnectionString()) { Database = _database + "_loans" }.ConnectionString);
        }
        else
        {
            builder.UseSqlServer(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(fixture.GetConnectionString()) { InitialCatalog = _database + "_loans" }.ConnectionString);
        }

        return builder.Options;
    }

    private DbContextOptions<AccountingDBContext> AccountingOptions()
    {
        var builder = new DbContextOptionsBuilder<AccountingDBContext>();

        if (fixture is PostgreSqlDbFixture)
        {
            builder.UseNpgsql(new Npgsql.NpgsqlConnectionStringBuilder(fixture.GetConnectionString()) { Database = _database + "_accounting" }.ConnectionString);
        }
        else
        {
            builder.UseSqlServer(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(fixture.GetConnectionString()) { InitialCatalog = _database + "_accounting" }.ConnectionString);
        }

        return builder.Options;
    }

    private async Task<Guid> SeedLoanAsync(Guid firstMember, Guid secondMember)
    {
        await using var db = new LoansDBContext(LoanOptions(), _tenant, _actor);
        await db.Database.EnsureCreatedAsync();
        var product = LoanProduct.Create(
            _tenant.TenantId,
            "GUAR-LOAN",
            "Guaranteed loan",
            null,
            12,
            InterestMethod.ReducingBalance,
            36,
            1000000,
            ActorId);

        var loan = LoanApplication.Create(
            _tenant.TenantId,
            "SYNTHETIC-LOAN",
            Guid.CreateVersion7(),
            product.Id,
            200000,
            36,
            12,
            InterestMethod.ReducingBalance,
            ActorId);

        loan.Status = LoanStatus.Approved;
        loan.ApplicationDate = _clock.Now;

        foreach (var member in new[] { firstMember, secondMember })
        {
            var pledge = LoanGuarantor.Create(_tenant.TenantId, loan.Id, member, 100000, ActorId);
            pledge.CreatedAt = _clock.Now;
            loan.Guarantors.Add(pledge);
        }
        db.AddRange(product, loan);
        await db.SaveChangesAsync();
        return loan.Id;
    }

    private async Task<Guid> PledgeIdAsync(Guid loanId, Guid memberId)
    {
        await using var db = new LoansDBContext(LoanOptions(), _tenant, _actor);
        return await db.LoanGuarantors.Where(x => x.LoanApplicationId == loanId && x.GuarantorMemberId == memberId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();
    }

    private static async Task<TResponse> SendAsync<TResponse>(ServiceProvider services, IRequest<TResponse> request)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    private async Task<List<GuarantorReservationInstruction>> InstructionsAsync(string operation)
    {
        await using var db = new LoansDBContext(LoanOptions(), _tenant, _actor);
        var messages = await db.Set<OutboxMessage>().OrderBy(x => x.SequenceNumber).ToListAsync();
        var result = new List<GuarantorReservationInstruction>();

        foreach (var message in messages)
        {
            using var document = JsonDocument.Parse(message.Body);
            var body = document.RootElement.GetProperty("message");

            if (body.TryGetProperty("operation", out var name) && name.GetString() == operation)
            {
                result.Add(JsonSerializer.Deserialize<GuarantorReservationInstruction>(body.GetRawText(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!);
            }
        }
        return result;
    }

    private async Task DeliverAsync(ServiceProvider services, GuarantorReservationInstruction instruction)
    {
        var bankResult = await SendAsync(services, instruction);
        Assert.True(bankResult.IsSuccess, bankResult.Error?.Message);
        var applied = await SendAsync(services, new ApplyGuarantorReservationOutcomeCommand(bankResult.Data!));
        Assert.True(applied.IsSuccess, applied.Error?.Message);
    }

    private async Task<(Guid Account, Guid Member, Guid Product)> SeedAsync(decimal balance = 200000, decimal unexplained = 0)
    {
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        await db.Database.EnsureCreatedAsync();
        var product = SavingsProduct.Create("Guarantee savings", "GUAR-" + Interlocked.Increment(ref _productSequence), 0, 0, true, 0, null, null, false, ActorId);
        product.AllowsGuarantorReservations = true;
        var member = Guid.CreateVersion7();
        var account = SavingsAccount.Create(member, product.Id, ActorId);
        account.Balance = balance;
        account.LockedFunds = unexplained;
        db.AddRange(product, account);
        await db.SaveChangesAsync();
        return (account.Id, member, product.Id);
    }

    private GuarantorReservationInstruction Reserve(Guid account, Guid member, decimal amount = 100000, Guid? loan = null) =>
        new(
            _tenant.TenantId,
            Guid.CreateVersion7(),
            loan ?? Guid.CreateVersion7(),
            "Reserve",
            [new(Guid.CreateVersion7(), member, account, amount, "KES", "reference/0.1", _clock.Now.AddDays(30))],
            ActorId);

    private static async Task<AppResponse<GuarantorReservationOutcome>> SendAsync(ServiceProvider services, GuarantorReservationInstruction instruction)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(new ProcessGuarantorReservationCommand(instruction));
    }

    [Fact]
    public async Task Concurrent_duplicate_and_post_commit_replay_keep_one_hold_and_recorded_outcome()
    {
        var seed = await SeedAsync();
        await using var services = Services();
        var instruction = Reserve(seed.Account, seed.Member);
        var results = await Task.WhenAll(SendAsync(services, instruction), SendAsync(services, instruction));
        Assert.All(results, result => Assert.True(result.IsSuccess && result.Data!.Succeeded, result.Error?.Message));
        var replay = await SendAsync(services, instruction);
        Assert.Equal(results[0].Data!.Items.Single(), replay.Data!.Items.Single());
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Equal(100000, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
        Assert.Single(await db.GuarantorReservations.ToListAsync());
        Assert.Single(await db.GuarantorReservationReceipts.ToListAsync());
        Assert.Equal(3, await db.Set<OutboxMessage>().CountAsync());
    }

    [Fact]
    public async Task Mismatched_operation_and_pledge_replays_do_not_change_the_original_hold()
    {
        var seed = await SeedAsync();
        await using var services = Services();
        var instruction = Reserve(seed.Account, seed.Member);
        Assert.True((await SendAsync(services, instruction)).Data!.Succeeded);
        var changed = instruction with { Items = [instruction.Items[0] with { Amount = 120000 }] };
        var sameOperation = await SendAsync(services, changed);
        Assert.False(sameOperation.IsSuccess);
        Assert.Equal(ErrorType.Conflict, sameOperation.Error!.Type);
        var samePledge = await SendAsync(services, changed with { OperationId = Guid.CreateVersion7() });
        Assert.False(samePledge.Data!.Succeeded);
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Equal(100000, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
    }

    [Fact]
    public async Task Different_pledges_competing_for_capacity_cannot_over_reserve()
    {
        var seed = await SeedAsync(balance: 150000);
        await using var services = Services();
        var results = await Task.WhenAll(SendAsync(services, Reserve(seed.Account, seed.Member)), SendAsync(services, Reserve(seed.Account, seed.Member)));
        Assert.Single(results.Where(x => x.Data!.Succeeded));
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Equal(100000, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
        Assert.Equal(2, await db.GuarantorReservationReceipts.CountAsync());
    }

    [Fact]
    public async Task Release_before_reserve_is_terminal_and_repeated_release_has_no_extra_effect()
    {
        var seed = await SeedAsync();
        await using var services = Services();
        var instruction = Reserve(seed.Account, seed.Member);
        var release = instruction with { Operation = "Release", OperationId = Guid.CreateVersion7() };
        Assert.True((await SendAsync(services, release)).Data!.Succeeded);
        Assert.False((await SendAsync(services, instruction)).Data!.Succeeded);
        await SendAsync(services, release);
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Equal(0, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
        Assert.Equal(GuarantorReservationState.Released, (await db.GuarantorReservations.SingleAsync()).State);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Expiry_and_commit_resolve_to_expired_security_or_a_non_expiring_commit(bool commitFirst)
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        await using var services = Services();
        var loanId = Guid.CreateVersion7();
        var one = Reserve(first.Account, first.Member, loan: loanId);
        var two = Reserve(second.Account, second.Member, loan: loanId);
        await SendAsync(services, one);
        await SendAsync(services, two);
        var commit = one with { OperationId = Guid.CreateVersion7(), Operation = "Commit", Items = [one.Items[0], two.Items[0]], RequiredCoverage = 200000 };

        if (commitFirst)
        {
            Assert.True((await SendAsync(services, commit)).Data!.Succeeded);
        }

        _clock.Now = _clock.Now.AddDays(31);
        await SendAsync(services, one with { OperationId = Guid.CreateVersion7(), Operation = "Expire" });

        if (!commitFirst)
        {
            Assert.False((await SendAsync(services, commit)).Data!.Succeeded);
        }

        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        var held = await db.GuarantorReservations.SingleAsync(x => x.PledgeId == one.Items[0].PledgeId);
        Assert.Equal(commitFirst ? GuarantorReservationState.Committed : GuarantorReservationState.Expired, held.State);
        Assert.Equal(commitFirst ? 100000 : 0, (await db.SavingsAccounts.SingleAsync(x => x.Id == first.Account)).LockedFunds);
    }

    [Fact]
    public async Task Recovery_is_approved_once_and_preserves_remaining_coverage()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        await using var services = Services();
        var loanId = Guid.CreateVersion7();
        var one = Reserve(first.Account, first.Member, 150000, loanId);
        var two = Reserve(second.Account, second.Member, 150000, loanId);
        await SendAsync(services, one);
        await SendAsync(services, two);
        var commit = one with { OperationId = Guid.CreateVersion7(), Operation = "Commit", Items = [one.Items[0], two.Items[0]], RequiredCoverage = 300000 };
        Assert.True((await SendAsync(services, commit)).Data!.Succeeded);
        var recovery = commit with
        {
            OperationId = Guid.CreateVersion7(),
            Operation = "Recover",
            ApprovedBy = "00000000-0000-0000-0000-000000000002",
            EvidenceReference = "synthetic-approved-notices",
            Items = [one.Items[0] with { RecoveryAmount = 50000 }, two.Items[0] with { RecoveryAmount = 50000 }]
        };

        Assert.True((await SendAsync(services, recovery)).Data!.Succeeded);
        Assert.True((await SendAsync(services, recovery)).Data!.Succeeded);
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.All(
            await db.SavingsAccounts.ToListAsync(),
            account =>
            {
                Assert.Equal(150000, account.Balance);
                Assert.Equal(100000, account.LockedFunds);
            });

        Assert.Equal(2, await db.SavingsTransactions.CountAsync());
    }

    [Fact]
    public async Task Unexplained_legacy_holds_and_foreign_member_accounts_cannot_be_reserved()
    {
        var seed = await SeedAsync(unexplained: 50000);
        await using var services = Services();
        Assert.False((await SendAsync(services, Reserve(seed.Account, seed.Member))).Data!.Succeeded);
        Assert.False((await SendAsync(services, Reserve(seed.Account, Guid.CreateVersion7()))).Data!.Succeeded);
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Equal(50000, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
    }

    [Fact]
    public async Task Missing_account_is_durably_rejected_and_survives_duplicate_delivery()
    {
        await SeedAsync();
        await using var services = Services();
        var instruction = Reserve(Guid.CreateVersion7(), Guid.CreateVersion7());
        var results = await Task.WhenAll(SendAsync(services, instruction), SendAsync(services, instruction));
        Assert.All(results, result => Assert.False(result.Data!.Succeeded));
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Single(await db.GuarantorReservationReceipts.ToListAsync());
    }

    [Fact]
    public async Task MediatR_discovers_internal_validator_and_rejects_invalid_money_before_writing()
    {
        var seed = await SeedAsync();
        await using var services = Services();
        var result = await SendAsync(services, Reserve(seed.Account, seed.Member, -1));
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.ValidationErrors);
        await using var db = new BankingDBContext(Options(), _tenant, new Actor());
        Assert.Empty(await db.GuarantorReservations.ToListAsync());
        Assert.Empty(await db.Set<OutboxMessage>().ToListAsync());
    }

    [Fact]
    public async Task Consent_waits_for_Banking_and_disbursement_waits_for_committed_security_under_replay()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();
        _memberResolver.MemberId = first.Member;
        Assert.True((await SendAsync(services, new AcceptGuarantorPledgeCommand(loanId, first.Member, 100000, first.Account, true, "LLANSACCO-REFERENCE/0.1", await PledgeIdAsync(loanId, first.Member)))).IsSuccess);

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            var pending = await db.LoanGuarantors.SingleAsync(x => x.GuarantorMemberId == first.Member);
            Assert.Equal(GuarantorStatus.PendingReservation, pending.Status);
            Assert.Equal(0, pending.LockedAmount);
        }
        _memberResolver.MemberId = second.Member;
        Assert.True((await SendAsync(services, new AcceptGuarantorPledgeCommand(loanId, second.Member, 100000, second.Account, true, "LLANSACCO-REFERENCE/0.1", await PledgeIdAsync(loanId, second.Member)))).IsSuccess);
        Assert.False((await SendAsync(services, new DisburseLoanCommand(loanId))).IsSuccess);

        foreach (var instruction in await InstructionsAsync("Reserve"))
        {
            await DeliverAsync(services, instruction);
        }

        var requests = await Task.WhenAll(SendAsync(services, new DisburseLoanCommand(loanId)), SendAsync(services, new DisburseLoanCommand(loanId)));
        Assert.All(requests, result => Assert.True(result.IsSuccess, result.Error?.Message));
        var commit = Assert.Single(await InstructionsAsync("Commit"));

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            Assert.Equal(LoanStatus.DisbursementPending, (await db.LoanApplications.SingleAsync()).Status);
            Assert.Empty(await db.LoanRepaymentSchedules.ToListAsync());
        }
        // Simulate lost acknowledgment after Banking committed, then redeliver both the instruction and outcome.
        var committed = await SendAsync(services, commit);
        Assert.True(committed.Data!.Succeeded);
        await DeliverAsync(services, commit);
        await DeliverAsync(services, commit);

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            Assert.Equal(LoanStatus.Disbursed, (await db.LoanApplications.SingleAsync()).Status);
            var schedules = await db.LoanRepaymentSchedules.ToListAsync();
            Assert.Equal(36, schedules.Count);
            Assert.Equal(200000, schedules.Sum(x => x.PrincipalExpected));
            Assert.All(await db.LoanGuarantors.ToListAsync(), pledge => Assert.Equal(GuarantorStatus.Committed, pledge.Status));
        }
        Assert.True((await SendAsync(services, new ProcessRepaymentCommand { LoanApplicationId = loanId, Amount = 200000, ReceiptNumber = "SYN-SETTLEMENT" })).IsSuccess);

        foreach (var release in await InstructionsAsync("Release"))
        {
            await DeliverAsync(services, release);
            await DeliverAsync(services, release);
        }
        await using var bank = new BankingDBContext(Options(), _tenant, _actor);
        Assert.All(await bank.SavingsAccounts.ToListAsync(), account => Assert.Equal(0, account.LockedFunds));
    }

    [Fact]
    public async Task Cancellation_before_reserve_delivery_cannot_resurrect_the_hold_or_loan()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();
        _memberResolver.MemberId = first.Member;
        await SendAsync(services, new AcceptGuarantorPledgeCommand(loanId, first.Member, 100000, first.Account, true, "LLANSACCO-REFERENCE/0.1", await PledgeIdAsync(loanId, first.Member)));
        Assert.True((await SendAsync(services, new CancelGuaranteedLoanCommand(loanId, "Synthetic cancellation"))).IsSuccess);

        foreach (var release in await InstructionsAsync("Release"))
        {
            await DeliverAsync(services, release);
        }

        foreach (var reserve in await InstructionsAsync("Reserve"))
        {
            await DeliverAsync(services, reserve);
        }

        await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        Assert.Equal(LoanStatus.Cancelled, (await loans.LoanApplications.SingleAsync()).Status);
        Assert.Equal(GuarantorStatus.Released, (await loans.LoanGuarantors.SingleAsync(x => x.GuarantorMemberId == first.Member)).Status);
        await using var bank = new BankingDBContext(Options(), _tenant, _actor);
        Assert.All(await bank.SavingsAccounts.ToListAsync(), account => Assert.Equal(0, account.LockedFunds));
    }

    [Fact]
    public async Task Recovery_requires_a_second_actor_and_earmarks_debt_until_the_confirmed_outcome_is_applied_once()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();

        foreach (var seed in new[] { first, second })
        {
            _memberResolver.MemberId = seed.Member;
            await SendAsync(services, new AcceptGuarantorPledgeCommand(loanId, seed.Member, 100000, seed.Account, true, "LLANSACCO-REFERENCE/0.1", await PledgeIdAsync(loanId, seed.Member)));
        }

        foreach (var reserve in await InstructionsAsync("Reserve"))
        {
            await DeliverAsync(services, reserve);
        }

        await SendAsync(services, new DisburseLoanCommand(loanId));
        await DeliverAsync(services, Assert.Single(await InstructionsAsync("Commit")));

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            (await db.LoanApplications.SingleAsync()).Status = LoanStatus.Defaulted;
            await db.SaveChangesAsync();
        }
        var recoveryId = Guid.CreateVersion7();
        Assert.True((await SendAsync(services, new RequestGuarantorRecoveryCommand(loanId, recoveryId, 100000, "SYNTHETIC-NOTICE-AND-APPROVAL"))).IsSuccess);
        Assert.False((await SendAsync(services, new ApproveGuarantorRecoveryCommand(recoveryId))).IsSuccess);
        _actor.ActorId = "00000000-0000-0000-0000-000000000002";
        Assert.True((await SendAsync(services, new ApproveGuarantorRecoveryCommand(recoveryId))).IsSuccess);
        Assert.True((await SendAsync(services, new ApproveGuarantorRecoveryCommand(recoveryId))).IsSuccess);
        Assert.False((await SendAsync(services, new ProcessRepaymentCommand { LoanApplicationId = loanId, Amount = 150000, ReceiptNumber = "SYN-TOO-LARGE" })).IsSuccess);
        var recovery = Assert.Single(await InstructionsAsync("Recover"));
        await DeliverAsync(services, recovery);
        await DeliverAsync(services, recovery);
        await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        var loan = await loans.LoanApplications.SingleAsync();
        Assert.Equal(100000, loan.OutstandingPrincipal);
        Assert.Equal(0, loan.PendingRecoveryPrincipal);
        Assert.Equal("Completed", (await loans.GuarantorRecoveries.SingleAsync()).Status);
        Assert.Single(await loans.LoanRepayments.ToListAsync());
        await using var bank = new BankingDBContext(Options(), _tenant, _actor);
        Assert.All(
            await bank.SavingsAccounts.ToListAsync(),
            account =>
            {
                Assert.Equal(150000, account.Balance);
                Assert.Equal(50000, account.LockedFunds);
            });

        Assert.All(await bank.SavingsTransactions.ToListAsync(), movement => Assert.Equal(recoveryId, movement.GuarantorOperationId));
    }

    [Fact]
    public async Task Equivalent_money_scale_and_time_offsets_replay_the_recorded_receipt()
    {
        var seed = await SeedAsync();
        await using var services = Services();
        var instruction = Reserve(seed.Account, seed.Member, 100000.00m);
        var first = await SendAsync(services, instruction);
        var replay = await SendAsync(
            services,
            instruction with
            {
                Operation = "reserve",
                Items = [instruction.Items[0] with
                { Amount = 100000m, ExpiresAt = instruction.Items[0].ExpiresAt.ToOffset(TimeSpan.FromHours(3)) }]
            });

        Assert.True(replay.IsSuccess, replay.Error?.Message);
        Assert.Equal(first.Data!.Items.Single(), replay.Data!.Items.Single());
        await using var db = new BankingDBContext(Options(), _tenant, _actor);
        Assert.Single(await db.GuarantorReservationReceipts.ToListAsync());
    }

    [Fact]
    public async Task A_foreign_tenant_envelope_cannot_observe_or_change_an_existing_hold()
    {
        var seed = await SeedAsync();
        var instruction = Reserve(seed.Account, seed.Member);

        await using (var own = Services())
        {
            Assert.True((await SendAsync(own, instruction)).Data!.Succeeded);
        }

        var foreign = new Tenant(Guid.CreateVersion7());
        await using var services = Services(tenant: foreign);
        Assert.False((await SendAsync(services, instruction)).IsSuccess);
        Assert.False((await SendAsync(services, instruction with { TenantId = foreign.TenantId })).Data!.Succeeded);
        await using var db = new BankingDBContext(Options(), _tenant, _actor);
        Assert.Equal(100000, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
        Assert.Single(await db.GuarantorReservationReceipts.ToListAsync());
        await using var other = new BankingDBContext(Options(), foreign, _actor);
        Assert.Equal(GuarantorReservationState.Rejected, (await other.GuarantorReservations.SingleAsync()).State);
    }

    private sealed class FailAfterSave : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default) => throw new InvalidOperationException("Synthetic failure before commit.");
    }

    [Fact]
    public async Task A_failure_after_save_rolls_back_the_hold_receipt_and_outbox_together()
    {
        var seed = await SeedAsync();
        var instruction = Reserve(seed.Account, seed.Member);

        await using (var broken = Services(new FailAfterSave()))
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync(broken, instruction));
        }

        await using (var db = new BankingDBContext(Options(), _tenant, _actor))
        {
            Assert.Equal(0, (await db.SavingsAccounts.SingleAsync()).LockedFunds);
            Assert.Empty(await db.GuarantorReservations.ToListAsync());
            Assert.Empty(await db.GuarantorReservationReceipts.ToListAsync());
            Assert.Empty(await db.Set<OutboxMessage>().ToListAsync());
        }
        await using var healthy = Services();
        Assert.True((await SendAsync(healthy, instruction)).Data!.Succeeded);
    }

    [Fact]
    public async Task Concurrent_credit_approvals_count_undisbursed_commitments_and_capture_the_eligible_deposit_policy()
    {
        var seed = await SeedAsync(balance: 100000);

        await using (var bank = new BankingDBContext(Options(), _tenant, _actor))
        {
            var product = await bank.SavingsProducts.SingleAsync();
            product.CountsTowardLoanLimit = true;
            product.AllowsWithdrawals = false;
            await bank.SaveChangesAsync();
        }
        await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        await loans.Database.EnsureCreatedAsync();
        var credit = LoanProduct.Create(_tenant.TenantId, "CREDIT", "Credit limit", null, 12, InterestMethod.ReducingBalance, 36, 1000000, ActorId);
        var applications = Enumerable.Range(1, 2).Select(i => LoanApplication.Create(
            _tenant.TenantId,
            "LIMIT-" + i,
            seed.Member,
            credit.Id,
            200000,
            36,
            12,
            InterestMethod.ReducingBalance,
            ActorId))
            .ToArray();

        foreach (var loan in applications)
        {
            loan.Status = LoanStatus.Submitted;
        }

        loans.Add(credit);
        loans.AddRange(applications);
        await loans.SaveChangesAsync();
        await using var services = Services();
        Assert.False((await SendAsync(services, new ApproveLoanCommand { LoanApplicationId = applications[0].Id })).IsSuccess);
        _actor.ActorId = "00000000-0000-0000-0000-000000000002";
        var results = await Task.WhenAll(applications.Select(loan => SendAsync(services, new ApproveLoanCommand { LoanApplicationId = loan.Id })));
        Assert.Single(results.Where(x => x.IsSuccess));
        loans.ChangeTracker.Clear();
        var approved = await loans.LoanApplications.SingleAsync(x => x.Status == LoanStatus.Approved);
        Assert.Equal(300000, approved.ApprovedExposureLimit);
        Assert.Equal(100000, approved.ApprovedEligibleDeposits);
        Assert.Equal(3, approved.ApprovedDepositMultiple);
    }

    [Fact]
    public async Task Policy_changes_require_fresh_displayed_terms_and_preserve_existing_consent_snapshots()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();
        _memberResolver.MemberId = first.Member;
        Assert.True((await SendAsync(
            services,
            new AcceptGuarantorPledgeCommand(
                loanId,
                first.Member,
                100000,
                first.Account,
                true,
                "LLANSACCO-REFERENCE/0.1",
                await PledgeIdAsync(loanId, first.Member)))).IsSuccess);

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            var product = await db.LoanProducts.SingleAsync();
            product.GuarantorPolicyVersion = "reference/0.2";
            product.GuarantorReservationDays = 7;
            await db.SaveChangesAsync();
        }
        _memberResolver.MemberId = second.Member;
        var stale = new AcceptGuarantorPledgeCommand(
            loanId,
            second.Member,
            100000,
            second.Account,
            true,
            "LLANSACCO-REFERENCE/0.1",
            await PledgeIdAsync(loanId, second.Member));

        Assert.False((await SendAsync(services, stale)).IsSuccess);
        Assert.True((await SendAsync(services, stale with { TermsVersion = "reference/0.2" })).IsSuccess);
        var instructions = await InstructionsAsync("Reserve");
        Assert.Contains(instructions, x => x.Items[0].PolicyVersion == "LLANSACCO-REFERENCE/0.1" && x.Items[0].ExpiresAt == _clock.Now.AddDays(30));
        Assert.Contains(instructions, x => x.Items[0].PolicyVersion == "reference/0.2" && x.Items[0].ExpiresAt == _clock.Now.AddDays(7));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Legacy_adoption_explains_existing_locks_without_incrementing_and_releases_closed_loans(bool closed, bool closeWhilePending)
    {
        var first = await SeedAsync(unexplained: 100000);
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        var pledgeId = await PledgeIdAsync(loanId, first.Member);

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            (await db.LoanApplications.SingleAsync()).Status = closed ? LoanStatus.Closed : LoanStatus.Active;
            var pledge = await db.LoanGuarantors.SingleAsync(x => x.Id == pledgeId);
            pledge.Status = GuarantorStatus.Accepted;
            pledge.LockedAmount = 100000;
            await db.SaveChangesAsync();
        }
        await using var services = Services();
        var reviewId = Guid.CreateVersion7();
        Assert.True((await SendAsync(
            services,
            new RequestGuarantorSecurityReviewCommand(
                reviewId,
                loanId,
                pledgeId,
                "AdoptLegacy",
                "SYNTHETIC-SOURCE-LOCK",
                first.Account,
                null))).IsSuccess);

        Assert.False((await SendAsync(services, new ApproveGuarantorSecurityReviewCommand(reviewId))).IsSuccess);
        _actor.ActorId = "00000000-0000-0000-0000-000000000002";
        Assert.True((await SendAsync(services, new ApproveGuarantorSecurityReviewCommand(reviewId))).IsSuccess);
        var adoption = Assert.Single(await InstructionsAsync("AdoptLegacy"));

        if (closeWhilePending)
        {
            await using var closing = new LoansDBContext(LoanOptions(), _tenant, _actor);
            (await closing.LoanApplications.SingleAsync()).Status = LoanStatus.Closed;
            await closing.SaveChangesAsync();
        }
        await DeliverAsync(services, adoption);
        await DeliverAsync(services, adoption);

        foreach (var release in await InstructionsAsync("Release"))
        {
            await DeliverAsync(services, release);
        }

        await using var bank = new BankingDBContext(Options(), _tenant, _actor);
        Assert.Equal(closed || closeWhilePending ? 0 : 100000, (await bank.SavingsAccounts.SingleAsync(x => x.Id == first.Account)).LockedFunds);
        Assert.Equal(
            closed || closeWhilePending ? GuarantorReservationState.Released : GuarantorReservationState.Committed,
            (await bank.GuarantorReservations.SingleAsync()).State);

        Assert.Empty(await bank.SavingsTransactions.ToListAsync());
    }

    [Fact]
    public async Task Substitution_keeps_the_original_security_until_replacement_consent_hold_and_independent_commit()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var replacement = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();

        foreach (var member in new[] { first, second })
        {
            _memberResolver.MemberId = member.Member;
            await SendAsync(
                services,
                new AcceptGuarantorPledgeCommand(
                    loanId,
                    member.Member,
                    100000,
                    member.Account,
                    true,
                    "LLANSACCO-REFERENCE/0.1",
                    await PledgeIdAsync(loanId, member.Member)));
        }

        foreach (var reserve in await InstructionsAsync("Reserve"))
        {
            await DeliverAsync(services, reserve);
        }

        await SendAsync(services, new DisburseLoanCommand(loanId));
        await DeliverAsync(services, Assert.Single(await InstructionsAsync("Commit")));
        var reviewId = Guid.CreateVersion7();
        Assert.True((await SendAsync(
            services,
            new RequestGuarantorSecurityReviewCommand(
                reviewId,
                loanId,
                await PledgeIdAsync(loanId, first.Member),
                "Substitute",
                "SYNTHETIC-SUBSTITUTION",
                null,
                replacement.Member))).IsSuccess);

        _actor.ActorId = "00000000-0000-0000-0000-000000000002";
        Assert.False((await SendAsync(services, new ApproveGuarantorSecurityReviewCommand(reviewId))).IsSuccess);
        _memberResolver.MemberId = replacement.Member;
        Assert.True((await SendAsync(
            services,
            new AcceptGuarantorPledgeCommand(
                loanId,
                replacement.Member,
                100000,
                replacement.Account,
                true,
                "LLANSACCO-REFERENCE/0.1",
                await PledgeIdAsync(loanId, replacement.Member)))).IsSuccess);

        var replacementReserve = (await InstructionsAsync("Reserve")).Single(x => x.Items[0].MemberId == replacement.Member);
        await DeliverAsync(services, replacementReserve);
        Assert.True((await SendAsync(services, new ApproveGuarantorSecurityReviewCommand(reviewId))).IsSuccess);
        Assert.Empty(await InstructionsAsync("Release"));
        var commit = (await InstructionsAsync("Commit")).Single(x => x.OperationId == reviewId);
        await DeliverAsync(services, commit);
        await DeliverAsync(services, commit);
        var release = Assert.Single(await InstructionsAsync("Release"));
        await DeliverAsync(services, release);
        await using var bank = new BankingDBContext(Options(), _tenant, _actor);
        Assert.Equal(0, (await bank.SavingsAccounts.SingleAsync(x => x.Id == first.Account)).LockedFunds);
        Assert.Equal(100000, (await bank.SavingsAccounts.SingleAsync(x => x.Id == replacement.Account)).LockedFunds);
        await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        Assert.Equal(36, await loans.LoanRepaymentSchedules.CountAsync());
        Assert.Equal("Completed", (await loans.GuarantorSecurityReviews.SingleAsync()).Status);
        Assert.True((await SendAsync(services, new ProcessRepaymentCommand { LoanApplicationId = loanId, Amount = 200000, ReceiptNumber = "SUBSTITUTED-CLOSE" })).IsSuccess);

        foreach (var settlementRelease in await InstructionsAsync("Release"))
        {
            await DeliverAsync(services, settlementRelease);
        }

        loans.ChangeTracker.Clear();
        Assert.All(await loans.LoanGuarantors.ToListAsync(), pledge => Assert.Equal(GuarantorStatus.Released, pledge.Status));
    }

    [Fact]
    public void Reducing_balance_schedule_matches_reference_money_and_month_end_dates()
    {
        var loan = LoanApplication.Create(
            _tenant.TenantId,
            "GOLDEN",
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            300000,
            36,
            12,
            InterestMethod.ReducingBalance,
            ActorId);

        loan.DisbursementDate = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var schedule = new ReducingBalanceAmortizationStrategy().GenerateSchedule(loan);
        Assert.Equal(300000, schedule.Sum(x => x.PrincipalExpected));
        Assert.Equal(3000, schedule[0].InterestExpected);
        Assert.Equal(6964.29m, schedule[0].PrincipalExpected);
        Assert.Equal(28, schedule[0].DueDate.Day);
        Assert.Equal(31, schedule[1].DueDate.Day);
        Assert.All(schedule, item => Assert.Equal(ActorId, item.CreatedBy));
    }

    [Theory]
    [InlineData("correct")]
    [InlineData("cash")]
    [InlineData("missing")]
    public async Task Recovery_posts_one_internal_offset_with_approved_mapping_and_quarantines_missing_or_cash_mappings(string mappingKind)
    {
        var liability = Account.Create(_tenant.TenantId, "SYN-DEP", "Guarantor deposits", AccountType.Liability, null, ActorId);
        var principal = Account.Create(_tenant.TenantId, "SYN-LOAN", "Loan principal", AccountType.Asset, null, ActorId);

        await using (var db = new AccountingDBContext(AccountingOptions(), _tenant, _actor))
        {
            await db.Database.EnsureCreatedAsync();
            db.AddRange(liability, principal);

            if (mappingKind != "missing")
            {
                db.Add(new TransactionTypeGlMapping
                {
                    TenantId = _tenant.TenantId,
                    TransactionTypeCode = "GUARANTOR_RECOVERY",
                    CreatedBy = ActorId,
                    DebitSideSource = mappingKind == "cash" ? "CHANNEL_ACCOUNT" : "EXPLICIT_GL",
                    DebitExplicitGlAccountId = liability.Id,
                    CreditSideSource = "EXPLICIT_GL",
                    CreditExplicitGlAccountId = principal.Id,
                    FeeSideSource = "EXPLICIT_GL"
                });
            }

            await db.SaveChangesAsync();
        }
        await using var services = Services();
        var recoveryId = Guid.CreateVersion7();
        var message = new GuarantorRecoveryCompletedIntegrationEvent(
            _tenant.TenantId,
            Guid.CreateVersion7(),
            recoveryId,
            100000,
            "GR-" + recoveryId.ToString("N"),
            ActorId)
        { EventId = recoveryId, OccurredAt = _clock.Now };

        for (var delivery = 0; delivery < 2; delivery++)
        {
            await using var scope = services.CreateAsyncScope();
            var context = Substitute.For<ConsumeContext<GuarantorRecoveryCompletedIntegrationEvent>>();
            context.Message.Returns(message);
            context.MessageId.Returns(recoveryId);
            await scope.ServiceProvider.GetRequiredService<GuarantorRecoveryCompletedIntegrationEventHandler>().Consume(context);
        }
        await using var verify = new AccountingDBContext(AccountingOptions(), _tenant, _actor);

        if (mappingKind == "correct")
        {
            var journal = await verify.Set<Journal>().Include(x => x.Lines).SingleAsync();
            Assert.Equal(100000, journal.Lines.Sum(x => x.Debit));
            Assert.Equal(100000, journal.Lines.Sum(x => x.Credit));
            Assert.Contains(journal.Lines, x => x.AccountId == liability.Id && x.Debit == 100000);
            Assert.Contains(journal.Lines, x => x.AccountId == principal.Id && x.Credit == 100000);
            Assert.Empty(await verify.Set<AccountingIntegrationError>().ToListAsync());
        }
        else
        {
            Assert.Empty(await verify.Set<Journal>().ToListAsync());
            Assert.NotEmpty(await verify.Set<AccountingIntegrationError>().ToListAsync());
        }
    }

    [Fact]
    public async Task Consent_rejects_another_member_or_unverified_identity_and_an_empty_security_list_cannot_disburse()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();
        var consent = new AcceptGuarantorPledgeCommand(
            loanId,
            first.Member,
            100000,
            first.Account,
            true,
            "LLANSACCO-REFERENCE/0.1",
            await PledgeIdAsync(loanId, first.Member));

        _memberResolver.MemberId = second.Member;
        Assert.False((await SendAsync(services, consent)).IsSuccess);
        _memberResolver.MemberId = first.Member;
        _memberResolver.Verified = false;
        Assert.False((await SendAsync(services, consent)).IsSuccess);
        await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        Assert.Empty(await loans.Set<OutboxMessage>().ToListAsync());
        var product = await loans.LoanProducts.SingleAsync();
        var unsecured = LoanApplication.Create(
            _tenant.TenantId,
            "NO-SECURITY",
            Guid.CreateVersion7(),
            product.Id,
            100000,
            12,
            12,
            InterestMethod.ReducingBalance,
            ActorId);

        unsecured.Status = LoanStatus.Approved;
        loans.Add(unsecured);
        await loans.SaveChangesAsync();
        Assert.False((await SendAsync(services, new DisburseLoanCommand(unsecured.Id))).IsSuccess);
        Assert.Empty(await loans.LoanRepaymentSchedules.ToListAsync());
    }

    [Fact]
    public async Task Competing_recovery_approvals_cannot_earmark_more_than_the_debt_and_unapproved_requests_can_be_cancelled()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        await using var services = Services();

        foreach (var member in new[] { first, second })
        {
            _memberResolver.MemberId = member.Member;
            Assert.True((await SendAsync(
                services,
                new AcceptGuarantorPledgeCommand(
                    loanId,
                    member.Member,
                    100000,
                    member.Account,
                    true,
                    "LLANSACCO-REFERENCE/0.1",
                    await PledgeIdAsync(loanId, member.Member)))).IsSuccess);
        }

        foreach (var reserve in await InstructionsAsync("Reserve"))
        {
            await DeliverAsync(services, reserve);
        }

        await SendAsync(services, new DisburseLoanCommand(loanId));
        await DeliverAsync(services, Assert.Single(await InstructionsAsync("Commit")));

        await using (var db = new LoansDBContext(LoanOptions(), _tenant, _actor))
        {
            (await db.LoanApplications.SingleAsync()).Status = LoanStatus.Defaulted;
            await db.SaveChangesAsync();
        }
        var ids = new[] { Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7() };

        foreach (var id in ids)
        {
            Assert.True((await SendAsync(services, new RequestGuarantorRecoveryCommand(loanId, id, 150000, "COMPETING-NOTICES"))).IsSuccess);
        }

        Assert.True((await SendAsync(services, new CancelGuarantorRecoveryCommand(ids[2]))).IsSuccess);
        _actor.ActorId = "00000000-0000-0000-0000-000000000002";
        Assert.False((await SendAsync(services, new ApproveGuarantorRecoveryCommand(ids[2]))).IsSuccess);
        var decisions = await Task.WhenAll(ids.Take(2).Select(id => SendAsync(services, new ApproveGuarantorRecoveryCommand(id))));
        Assert.Single(decisions, x => x.IsSuccess);
        var recovery = Assert.Single(await InstructionsAsync("Recover"));
        Assert.False((await SendAsync(services, new CancelGuarantorRecoveryCommand(recovery.OperationId))).IsSuccess);
        await DeliverAsync(services, recovery);
        await DeliverAsync(services, recovery);
        await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        var loan = await loans.LoanApplications.SingleAsync();
        Assert.Equal(50000, loan.OutstandingPrincipal);
        Assert.Equal(0, loan.PendingRecoveryPrincipal);
        Assert.Single(await loans.LoanRepayments.ToListAsync());
    }

    private DbContext MigrationContext(bool loans)
    {
        if (fixture is PostgreSqlDbFixture)
        {
            var connection = new Npgsql.NpgsqlConnectionStringBuilder(fixture.GetConnectionString())
            { Database = loans ? _database + "_loans" : _database }.ConnectionString;

            return loans
                ? new LoansPostgreSqlDBContext(
                    new DbContextOptionsBuilder<LoansPostgreSqlDBContext>()
                        .UseNpgsql(connection, provider => provider.MigrationsHistoryTable("__EFMigrationsHistory_Loans"))
                        .ReplaceService<IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>().Options,
                    _tenant,
                    _actor)
                : new BankingPostgreSqlDBContext(
                    new DbContextOptionsBuilder<BankingPostgreSqlDBContext>()
                        .UseNpgsql(connection, provider => provider.MigrationsHistoryTable("__EFMigrationsHistory_Banking"))
                        .ReplaceService<IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>().Options,
                    _tenant,
                    _actor);
        }
        var sql = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(fixture.GetConnectionString())
        { InitialCatalog = loans ? _database + "_loans" : _database }.ConnectionString;

        return loans
            ? new LoansSqlServerDBContext(
                new DbContextOptionsBuilder<LoansSqlServerDBContext>()
                    .UseSqlServer(sql, provider => provider.MigrationsHistoryTable("__EFMigrationsHistory_Loans"))
                    .ReplaceService<IMigrationsSqlGenerator, IdempotentSqlServerMigrationsSqlGenerator>().Options,
                _tenant,
                _actor)
            : new BankingSqlServerDBContext(
                new DbContextOptionsBuilder<BankingSqlServerDBContext>()
                    .UseSqlServer(sql, provider => provider.MigrationsHistoryTable("__EFMigrationsHistory_Banking"))
                    .ReplaceService<IMigrationsSqlGenerator, IdempotentSqlServerMigrationsSqlGenerator>().Options,
                _tenant,
                _actor);
    }

    [Fact]
    public async Task Provider_migrations_install_and_upgrade_prior_schema_without_losing_unexplained_legacy_holds()
    {
        await using var bank = MigrationContext(false);
        await using var loans = MigrationContext(true);
        Assert.False(bank.Database.HasPendingModelChanges());
        Assert.False(loans.Database.HasPendingModelChanges());
        await bank.Database.MigrateAsync();
        await loans.Database.MigrateAsync();
        var seed = await SeedAsync(unexplained: 75000);
        var loanId = await SeedLoanAsync(seed.Member, Guid.CreateVersion7());

        foreach (var db in new[] { bank, loans })
        {
            var migrations = db.Database.GetMigrations().ToArray();
            await db.GetService<IMigrator>().MigrateAsync(migrations[^2]);
            await db.Database.MigrateAsync();
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        }
        await using var current = new BankingDBContext(Options(), _tenant, _actor);
        Assert.Equal(75000, (await current.SavingsAccounts.SingleAsync()).LockedFunds);
        Assert.False((await current.SavingsProducts.SingleAsync()).AllowsGuarantorReservations);
        Assert.Empty(await current.GuarantorReservations.ToListAsync());
        await using var currentLoans = new LoansDBContext(LoanOptions(), _tenant, _actor);
        Assert.Equal(loanId, (await currentLoans.LoanApplications.SingleAsync()).Id);
        Assert.Equal(2, await currentLoans.LoanGuarantors.CountAsync());
    }

    private IHost BrokerHost(string prefix)
    {
        var builder = Host.CreateApplicationBuilder();
        ConfigureServices(builder.Services, brokerPrefix: prefix);
        return builder.Build();
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> predicate)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(45));

        while (!await predicate())
        {
            await Task.Delay(100, deadline.Token);
        }
    }

    [RabbitMqFact]
    [Trait("Category", "ExternalRabbitMq")]
    public async Task Real_broker_recovers_pending_consent_and_disbursement_from_durable_context_outboxes_after_restart()
    {
        var first = await SeedAsync();
        var second = await SeedAsync();
        var loanId = await SeedLoanAsync(first.Member, second.Member);
        var prefix = "llansacco-guarantees-" + Guid.CreateVersion7().ToString("N");

        using (var stopped = BrokerHost(prefix))
        {
            foreach (var member in new[] { first, second })
            {
                _memberResolver.MemberId = member.Member;
                await using var scope = stopped.Services.CreateAsyncScope();
                var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new AcceptGuarantorPledgeCommand(
                    loanId,
                    member.Member,
                    100000,
                    member.Account,
                    true,
                    "LLANSACCO-REFERENCE/0.1",
                    await PledgeIdAsync(loanId, member.Member)));

                Assert.True(result.IsSuccess, result.Error?.Message);
            }
            await using var bank = new BankingDBContext(Options(), _tenant, _actor);
            Assert.Empty(await bank.GuarantorReservations.ToListAsync());
        }

        using (var running = BrokerHost(prefix))
        {
            await running.StartAsync();

            try
            {
                await WaitUntilAsync(async () =>
                {
                    await using var db = new LoansDBContext(LoanOptions(), _tenant, _actor);
                    return await db.LoanGuarantors.CountAsync(x => x.Status == GuarantorStatus.ReservationHeld) == 2;
                });
            }
            finally
            {
                await running.StopAsync();
            }
        }
        // Persist the next decision with the transport stopped, then use a newly built host and scopes.
        using (var stopped = BrokerHost(prefix))
        {
            await using var scope = stopped.Services.CreateAsyncScope();
            Assert.True((await scope.ServiceProvider.GetRequiredService<ISender>().Send(new DisburseLoanCommand(loanId))).IsSuccess);
        }
        using var restarted = BrokerHost(prefix);
        await restarted.StartAsync();

        try
        {
            await WaitUntilAsync(async () =>
            {
                await using var db = new LoansDBContext(LoanOptions(), _tenant, _actor);
                return await db.LoanApplications.AnyAsync(x => x.Id == loanId && x.Status == LoanStatus.Disbursed);
            });

            await using (var scope = restarted.Services.CreateAsyncScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                Assert.True((await sender.Send(new ProcessRepaymentCommand { LoanApplicationId = loanId, Amount = 200000, ReceiptNumber = "BROKER-CLOSE" })).IsSuccess);
            }
            await WaitUntilAsync(async () =>
            {
                await using var db = new BankingDBContext(Options(), _tenant, _actor);
                return !await db.SavingsAccounts.AnyAsync(x => x.LockedFunds != 0);
            });

            await using var loans = new LoansDBContext(LoanOptions(), _tenant, _actor);
            Assert.Equal(36, await loans.LoanRepaymentSchedules.CountAsync());
            Assert.Equal(LoanStatus.Closed, (await loans.LoanApplications.SingleAsync()).Status);
            await using var bank = new BankingDBContext(Options(), _tenant, _actor);
            Assert.Equal(2, await bank.GuarantorReservations.CountAsync(x => x.State == GuarantorReservationState.Released));
        }
        finally
        {
            await restarted.StopAsync();
        }
    }

    private sealed class SaveBarrier : SaveChangesInterceptor
    {
        private int _arrivals;
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _withdrawalCommitted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void ConfirmWithdrawalCommitted() => _withdrawalCommitted.TrySetResult();

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var arrival = Interlocked.Increment(ref _arrivals);

            if (arrival == 2)
            {
                _ready.TrySetResult();
            }

            if (arrival <= 2)
            {
                await _ready.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);

                if (eventData.Context!.ChangeTracker.Entries<GuarantorReservationReceipt>().Any(x => x.State == EntityState.Added))
                {
                    await _withdrawalCommitted.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
                }
            }

            return result;
        }
    }

    [Fact]
    public async Task Forced_same_snapshot_withdrawal_race_cannot_spend_guaranteed_funds()
    {
        var seed = await SeedAsync();

        await using (var setup = Services())
        {
            await SendAsync(setup, Reserve(seed.Account, seed.Member, 50000));
        }

        var barrier = new SaveBarrier();
        await using var services = Services(barrier);
        var instruction = Reserve(seed.Account, seed.Member, 100000);
        var withdrawal = SendAsync(services, new WithdrawSavingsCommand(new WithdrawSavingsRequest(seed.Member, seed.Product, 80000, null, "SYN-RACE")));
        var reservation = SendAsync(services, instruction);
        var withdrawalResult = await withdrawal;
        barrier.ConfirmWithdrawalCommitted();
        var reservationResult = await reservation;
        Assert.True(withdrawalResult.IsSuccess);
        Assert.False(reservationResult.Data!.Succeeded);
        await using var db = new BankingDBContext(Options(), _tenant, _actor);
        var account = await db.SavingsAccounts.SingleAsync();
        Assert.True(account.Balance - account.LockedFunds >= 0);
        Assert.Equal(120000, account.Balance);
        Assert.Equal(50000, account.LockedFunds);
        Assert.Equal(3, await db.Set<OutboxMessage>().CountAsync());
    }
}
