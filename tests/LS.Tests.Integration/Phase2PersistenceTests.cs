using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using NSubstitute;
using LS.SharedKernel.Dtos.Common;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.CheckOff.Jobs;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Persistence.Features.Accounting.DataContext;
using LS.Persistence.Features.Accounting.Repositories;
using LS.Application.Features.Accounting.Services;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Persistence.Features.Loans;
using LS.Persistence.Features.Loans.DataContext;
using LS.Application.Features.Loans.LoanRepayments.Commands;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.Extensions.Hosting;
using LS.Application.Features.Banking.Savings.Jobs;
using LS.Application.Features.Banking.Deposits.Jobs;
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
    private static ServiceProvider Services<TContext>(TContext context, Tenant tenant, bool failSavingsPublication = false, TimeProvider? clock = null, bool failInterestPublication = false) where TContext : DbContext
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(context);
        services.AddScoped<LS.Application.Contracts.Interfaces.Common.IContextEventPublisher<IBankingUnitOfWork>, PersistenceTestPublisher>();
        services.AddSingleton<TimeProvider>(clock ?? TimeProvider.System);
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
        else if (context is AccountingDBContext)
        {
            foreach (var contract in typeof(IAccountingUnitOfWork).GetProperties().Select(p => p.PropertyType))
            {
                var implementation = typeof(AccountingUnitOfWork).Assembly.GetTypes().Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
                services.AddScoped(contract, sp => ActivatorUtilities.CreateInstance(sp, implementation));
            }
            services.AddScoped<IAccountingUnitOfWork, AccountingUnitOfWork>();
        }
        else if (context is LoansDBContext)
        {
            services.AddScoped<DbContext>(_ => context);
            services.AddScoped(typeof(LS.Domain.Shared.Contracts.Repositories.IRepository<>), typeof(LS.Persistence.Common.Repositories.Repository<>));
            foreach (var contract in new[] { typeof(LS.Domain.Features.Loans.Contracts.Repositories.ILoanProductRepository), typeof(LS.Domain.Features.Loans.Contracts.Repositories.ILoanApplicationRepository), typeof(LS.Domain.Features.Loans.Contracts.Repositories.ILoanRepaymentRepository) })
            {
                var implementation = typeof(LoansUnitOfWork).Assembly.GetTypes().Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
                services.AddScoped(contract, sp => ActivatorUtilities.CreateInstance(sp, implementation));
            }
            services.AddScoped<ILoansUnitOfWork, LoansUnitOfWork>();
            services.AddScoped<LS.Application.Contracts.Interfaces.Common.IContextEventPublisher<ILoansUnitOfWork>, LoanTestPublisher>();
        }
        else services.AddScoped<ICheckOffUnitOfWork, CheckOffUnitOfWork>();
        if (failInterestPublication)
            services.AddSingleton<INotificationHandler<SavingsInterestAppliedIntegrationEvent>>(new RejectInterestPublication());
        return services.BuildServiceProvider();
    }

    // Persistence fault injection only; durable delivery is tested separately with the real bus outbox.
    private sealed class PersistenceTestPublisher(IPublisher publisher)
        : LS.Application.Contracts.Interfaces.Common.IContextEventPublisher<IBankingUnitOfWork>
    {
        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
            => publisher.Publish(message, cancellationToken);
    }

    private sealed class RejectSavingsPublication : INotificationHandler<SavingsDepositedIntegrationEvent>
    {
        public Task Handle(SavingsDepositedIntegrationEvent notification, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Injected publication failure.");
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RejectInterestPublication : INotificationHandler<SavingsInterestAppliedIntegrationEvent>
    {
        private int _calls;
        public Task Handle(SavingsInterestAppliedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            if (++_calls == 2) throw new InvalidOperationException("Injected second-account failure.");
            return Task.CompletedTask;
        }
    }

    private sealed class LoanTestPublisher(IPublisher publisher) : LS.Application.Contracts.Interfaces.Common.IContextEventPublisher<ILoansUnitOfWork>
    {
        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
            => publisher.Publish(message, cancellationToken);
    }

    private sealed class RejectRoleAssignment : IUserValidator<LS.Domain.Features.IAM.Users.Entities.AppUser>
    {
        private int _calls;
        public Task<IdentityResult> ValidateAsync(UserManager<LS.Domain.Features.IAM.Users.Entities.AppUser> manager,
            LS.Domain.Features.IAM.Users.Entities.AppUser user)
            => Task.FromResult(++_calls == 1 ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Code = "InjectedRoleRejection", Description = "Injected role rejection." }));
    }

    private sealed class CreationProfileFault(string mode, CancellationTokenSource cancellation) : SaveChangesInterceptor
    {
        public int Attempts { get; private set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<LS.Domain.Features.IAM.Users.Entities.AppUserProfile>()
                .Any(e => e.State == EntityState.Added))
            {
                Attempts++;
                if (mode == "failure") throw new InvalidOperationException("Injected profile save failure.");
                if (mode == "retry" && Attempts == 1) throw new DbUpdateConcurrencyException("Injected profile conflict.");
                if (mode == "cancel")
                {
                    cancellation.Cancel();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return ValueTask.FromResult(result);
        }
    }

    [Theory]
    [InlineData("success")]
    [InlineData("failure")]
    [InlineData("retry")]
    [InlineData("cancel")]
    [InlineData("role-rejection")]
    public async Task User_creation_roles_and_profile_are_atomic(string mode)
    {
        var tenant = new Tenant();
        using var cancellation = new CancellationTokenSource();
        var fault = new CreationProfileFault(mode, cancellation);
        var options = new DbContextOptionsBuilder<LS.Persistence.Features.IAM.DataContext.IamDBContext>(
            Options<LS.Persistence.Features.IAM.DataContext.IamDBContext>()).AddInterceptors(fault).Options;
        await using var db = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor());
        await db.Database.EnsureCreatedAsync();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(db);
        services.AddSingleton<ICurrentTenantProvider>(tenant);
        services.AddSingleton<ICurrentActorProvider>(new Actor());
        services.AddSingleton(Substitute.For<IHrUnitOfWork>());
        services.AddIdentityCore<LS.Domain.Features.IAM.Users.Entities.AppUser>()
            .AddRoles<LS.Domain.Features.IAM.Users.Entities.AppRole>()
            .AddEntityFrameworkStores<LS.Persistence.Features.IAM.DataContext.IamDBContext>();
        foreach (var contract in typeof(LS.Domain.Features.IAM.Contracts.IIamUnitOfWork).GetProperties()
            .Select(p => p.PropertyType).Where(t => !t.IsGenericType))
        {
            var implementation = typeof(LS.Persistence.Features.IAM.IamUnitOfWork).Assembly.GetTypes()
                .Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
            services.AddScoped(contract, sp => ActivatorUtilities.CreateInstance(sp, implementation));
        }
        services.AddScoped<LS.Domain.Features.IAM.Contracts.IIamUnitOfWork, LS.Persistence.Features.IAM.IamUnitOfWork>();
        services.AddSingleton(Substitute.For<IPublisher>());
        if (mode == "role-rejection")
            services.AddSingleton<IUserValidator<LS.Domain.Features.IAM.Users.Entities.AppUser>, RejectRoleAssignment>();
        await using var provider = services.BuildServiceProvider();
        var role = new LS.Domain.Features.IAM.Users.Entities.AppRole { Id = Guid.CreateVersion7().ToString(), Name = "CreationTest" };
        Assert.True((await provider.GetRequiredService<RoleManager<LS.Domain.Features.IAM.Users.Entities.AppRole>>().CreateAsync(role)).Succeeded);
        var handlerType = typeof(LS.Infrastructure.Features.IAM.Extensions.IamModuleDI).Assembly.GetType(
            "LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers.CreateAppUser")!;
        var handler = (IRequestHandler<LS.Application.Features.IAM.Users.Commands.CreateAppUserCommand,
            AppResponse<LS.SharedKernel.Features.IAM.Users.Dtos.AppUserResponse>>)ActivatorUtilities.CreateInstance(provider, handlerType);
        var command = new LS.Application.Features.IAM.Users.Commands.CreateAppUserCommand(
            new LS.SharedKernel.Features.IAM.Users.Dtos.CreateAppUserRequest("creation-test", "creation@example.test",
                "Creation-Test-Password1!", "Test", "User", "+254700000000", null, "Other", null, null, ["CreationTest"]), new Actor().ActorId);
        if (mode == "failure")
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellation.Token));
        else if (mode == "cancel")
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.Handle(command, cancellation.Token));
        else
            Assert.Equal(mode != "role-rejection", (await handler.Handle(command, cancellation.Token)).IsSuccess);
        Assert.Equal(mode == "retry" ? 2 : mode == "role-rejection" ? 0 : 1, fault.Attempts);
        // A later save on the same scope must not leak rolled-back entities.
        if (mode is "failure" or "cancel" or "role-rejection")
            Assert.Empty(db.ChangeTracker.Entries());
        await db.SaveChangesAsync();
        await using var verify = new LS.Persistence.Features.IAM.DataContext.IamDBContext(
            Options<LS.Persistence.Features.IAM.DataContext.IamDBContext>(), tenant, new Actor());
        var saved = await verify.Users.SingleOrDefaultAsync(u => u.UserName == "creation-test");
        if (mode is "failure" or "cancel" or "role-rejection")
        {
            Assert.Null(saved);
            Assert.Empty(await verify.AppUserProfiles.ToListAsync());
            Assert.Equal(0, await verify.UserRoles.CountAsync(r => r.RoleId == role.Id));
        }
        else
        {
            Assert.NotNull(saved);
            Assert.Equal(tenant.TenantId, saved.TenantId);
            Assert.Equal(new Actor().ActorId, saved.CreatedBy);
            Assert.Equal(saved.Id, (await verify.AppUserProfiles.SingleAsync()).AppUserId);
            Assert.Equal(1, await verify.UserRoles.CountAsync(r => r.UserId == saved.Id && r.RoleId == role.Id));
        }
    }
    private sealed class RejectResetMetadata : IUserValidator<LS.Domain.Features.IAM.Users.Entities.AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<LS.Domain.Features.IAM.Users.Entities.AppUser> manager,
            LS.Domain.Features.IAM.Users.Entities.AppUser user)
            => Task.FromResult(user.PasswordLastChanged is null ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Code = "InjectedMetadataRejection", Description = "Injected metadata rejection." }));
    }
    private sealed class ResetRevocationFault(string mode, CancellationTokenSource cancellation) : SaveChangesInterceptor
    {
        public int Attempts { get; private set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<LS.Domain.Features.IAM.Users.Entities.RefreshToken>()
                .Any(e => e.State == EntityState.Modified && e.Entity.RevokedAt != null))
            {
                Attempts++;
                if (mode == "failure") throw new InvalidOperationException("Injected token revocation failure.");
                if (mode == "retry" && Attempts == 1) throw new DbUpdateConcurrencyException("Injected revocation conflict.");
                if (mode == "cancel")
                {
                    cancellation.Cancel();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return ValueTask.FromResult(result);
        }
    }

    [Theory]
    [InlineData("success")]
    [InlineData("failure")]
    [InlineData("retry")]
    [InlineData("cancel")]
    [InlineData("identity-rejection")]
    public async Task Password_reset_and_token_revocation_share_one_transaction(string mode)
    {
        var tenant = new Tenant();
        using var cancellation = new CancellationTokenSource();
        var fault = new ResetRevocationFault(mode, cancellation);
        var options = new DbContextOptionsBuilder<LS.Persistence.Features.IAM.DataContext.IamDBContext>(
            Options<LS.Persistence.Features.IAM.DataContext.IamDBContext>()).AddInterceptors(fault).Options;
        await using var db = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor());
        await db.Database.EnsureCreatedAsync();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(db);
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddIdentityCore<LS.Domain.Features.IAM.Users.Entities.AppUser>()
            .AddRoles<LS.Domain.Features.IAM.Users.Entities.AppRole>()
            .AddEntityFrameworkStores<LS.Persistence.Features.IAM.DataContext.IamDBContext>()
            .AddDefaultTokenProviders();
        foreach (var contract in typeof(LS.Domain.Features.IAM.Contracts.IIamUnitOfWork).GetProperties()
            .Select(p => p.PropertyType).Where(t => !t.IsGenericType))
        {
            var implementation = typeof(LS.Persistence.Features.IAM.IamUnitOfWork).Assembly.GetTypes()
                .Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
            services.AddScoped(contract, sp => ActivatorUtilities.CreateInstance(sp, implementation));
        }
        services.AddScoped<LS.Domain.Features.IAM.Contracts.IIamUnitOfWork, LS.Persistence.Features.IAM.IamUnitOfWork>();
        if (mode == "identity-rejection")
            services.AddSingleton<IUserValidator<LS.Domain.Features.IAM.Users.Entities.AppUser>, RejectResetMetadata>();
        services.AddSingleton(Substitute.For<IPublisher>());
        services.AddSingleton(Substitute.For<ICacheService>());
        services.AddHttpContextAccessor();
        await using var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<LS.Domain.Features.IAM.Users.Entities.AppUser>>();
        var user = LS.Domain.Features.IAM.Users.Entities.AppUser.Create(tenant.TenantId, null, "reset-test", "Test", "User",
            "reset@example.test", "+254700000000", new Actor().ActorId);
        Assert.True((await manager.CreateAsync(user, "Original-Test-Password1!")).Succeeded);
        var originalHash = user.PasswordHash;
        var originalStamp = user.SecurityStamp;
        var originalChanged = user.PasswordLastChanged;
        var resetToken = await manager.GeneratePasswordResetTokenAsync(user);
        var refresh = LS.Domain.Features.IAM.Users.Entities.RefreshToken.Create(user.Id, "reset-test-refresh",
            DateTimeOffset.UtcNow.AddHours(1), user.Id);
        db.Add(refresh);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var handlerType = typeof(LS.Infrastructure.Features.IAM.Extensions.IamModuleDI).Assembly.GetType(
            "LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers.ResetPassword")!;
        var handler = (IRequestHandler<LS.Application.Features.IAM.Users.Commands.ResetPasswordCommand, AppResponse<bool>>)
            ActivatorUtilities.CreateInstance(provider, handlerType);
        var command = new LS.Application.Features.IAM.Users.Commands.ResetPasswordCommand(
            new LS.SharedKernel.Features.IAM.Users.Dtos.ResetPasswordRequest(user.Email!, null,
                "Replacement-Test-Password2!", "Replacement-Test-Password2!", resetToken));
        if (mode == "cancel")
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.Handle(command, cancellation.Token));
        else
            Assert.Equal(mode is not ("failure" or "identity-rejection"), (await handler.Handle(command, cancellation.Token)).IsSuccess);
        Assert.Equal(mode == "retry" ? 2 : mode == "identity-rejection" ? 0 : 1, fault.Attempts);
        await using var verify = new LS.Persistence.Features.IAM.DataContext.IamDBContext(
            Options<LS.Persistence.Features.IAM.DataContext.IamDBContext>(), tenant, new Actor());
        var savedUser = await verify.Users.SingleAsync(u => u.Id == user.Id);
        var savedToken = await verify.RefreshTokens.SingleAsync(t => t.Id == refresh.Id);
        if (mode is "failure" or "cancel" or "identity-rejection")
        {
            Assert.Equal(originalHash, savedUser.PasswordHash);
            Assert.Equal(originalStamp, savedUser.SecurityStamp);
            Assert.Equal(originalChanged, savedUser.PasswordLastChanged);
            Assert.Null(savedToken.RevokedAt);
        }
        else
        {
            Assert.NotEqual(originalHash, savedUser.PasswordHash);
            Assert.NotEqual(originalStamp, savedUser.SecurityStamp);
            Assert.NotNull(savedToken.RevokedAt);
        }
    }
    private sealed class CheckoffPaymentSender : IBackgroundRequestSender
    {
        public bool Reject { get; set; } = true;
        public List<string?> References { get; } = [];
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var command = Assert.IsType<DepositSavingsCommand>(request);
            References.Add(command.Request.ExternalReferenceId);
            var result = Reject ? AppResponses.Failure<SavingsAccountResponse>("Injected payment rejection")
                : AppResponses.Success<SavingsAccountResponse>(default!);
            return Task.FromResult((TResponse)(object)result);
        }
    }

    [Fact]
    public async Task Refresh_token_claim_is_atomic_tenant_scoped_and_rolled_back_with_its_transaction()
    {
        var tenant = new Tenant();
        var options = Options<LS.Persistence.Features.IAM.DataContext.IamDBContext>();
        var user = LS.Domain.Features.IAM.Users.Entities.AppUser.Create(tenant.TenantId, null, "rotation-test", "Test", "User", "rotation@example.test", "+254700000000", new Actor().ActorId);
        var token = LS.Domain.Features.IAM.Users.Entities.RefreshToken.Create(user.Id, "phase2-test-refresh", DateTimeOffset.UtcNow.AddHours(1), user.Id);
        LS.Domain.Features.IAM.Users.Contracts.Repositories.ITokenRepository Repository(LS.Persistence.Features.IAM.DataContext.IamDBContext db)
            => (LS.Domain.Features.IAM.Users.Contracts.Repositories.ITokenRepository)Activator.CreateInstance(
                typeof(LS.Persistence.Features.IAM.DataContext.IamDBContext).Assembly.GetType("LS.Persistence.Features.IAM.Users.Repositories.IamTokenRepository")!, db)!;
        await using (var setup = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(user, token);
            await setup.SaveChangesAsync();
        }
        await using (var otherTenant = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, new Tenant(), new Actor()))
            Assert.False(await Repository(otherTenant).TryUseRefreshTokenAsync(token.Token, user.Id));
        await using (var rolledBack = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor()))
        {
            await using var transaction = await rolledBack.Database.BeginTransactionAsync();
            Assert.True(await Repository(rolledBack).TryUseRefreshTokenAsync(token.Token, user.Id));
            await transaction.RollbackAsync();
        }
        await using var first = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor());
        await using var second = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor());
        var results = await Task.WhenAll(Repository(first).TryUseRefreshTokenAsync(token.Token, user.Id),
            Repository(second).TryUseRefreshTokenAsync(token.Token, user.Id));
        Assert.Single(results, success => success);
        Assert.Single(results, success => !success);
        await using var verify = new LS.Persistence.Features.IAM.DataContext.IamDBContext(options, tenant, new Actor());
        var persisted = await verify.RefreshTokens.SingleAsync(t => t.Id == token.Id);
        Assert.NotNull(persisted.UsedAt);
        Assert.Equal(user.Id, persisted.UpdatedBy);
    }
    [Fact]
    public async Task Share_transfer_creates_destination_and_rejects_self_transfer_without_changing_balance()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var product = LS.Domain.Features.Banking.Shares.Entities.ShareProduct.Create("Shares", "SHR", 10, 1, new Actor().ActorId);
        var source = LS.Domain.Features.Banking.Shares.Entities.ShareAccount.Create(Guid.CreateVersion7(), product.Id, new Actor().ActorId);
        source.AddShares(100, 10, new Actor().ActorId);
        var destination = Guid.CreateVersion7();
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(product, source);
            await setup.SaveChangesAsync();
        }
        await using (var db = new BankingDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(db, tenant);
            var sender = services.GetRequiredService<ISender>();
            Assert.False((await sender.Send(new LS.Application.Features.Banking.Shares.Commands.TransferSharesCommand(source.MemberId, source.MemberId, product.Id, 10, null))).IsSuccess);
            await services.GetRequiredService<IBankingUnitOfWork>().CompleteAsync();
            Assert.True((await sender.Send(new LS.Application.Features.Banking.Shares.Commands.TransferSharesCommand(source.MemberId, destination, product.Id, 10, null))).IsSuccess);
        }
        await using var verify = new BankingDBContext(options, tenant, new Actor());
        var accounts = await verify.ShareAccounts.ToListAsync();
        Assert.Equal(2, accounts.Count);
        Assert.Equal(90, accounts.Single(a => a.MemberId == source.MemberId).TotalShares);
        Assert.Equal(10, accounts.Single(a => a.MemberId == destination).TotalShares);
        Assert.Equal(1000m, accounts.Sum(a => a.TotalValue));
        Assert.Equal(2, await verify.ShareTransactions.CountAsync());
    }
    [Fact]
    public async Task Checkoff_worker_leaves_rejected_rows_retryable_and_finishes_only_after_success()
    {
        var tenant = new Tenant();
        var options = Options<CheckOffDBContext>();
        var employer = Employer.Create(tenant.TenantId, "Employer", "Contact", "employer@example.test", "+254700000000", new Actor().ActorId);
        var member = Member.Create(tenant.TenantId, "M1", "One", "Member", "m1@example.test", "+254700000001", "ID1", new DateOnly(1990, 1, 1), default, new Actor().ActorId);
        var batch = CheckoffBatch.Create(tenant.TenantId, employer.Id, "POST", DateTime.UtcNow, 100, new Actor().ActorId);
        batch.Status = CheckoffBatchStatus.Posting;
        var row = CheckoffStagingRow.Create(tenant.TenantId, batch.Id, "E1", "One", 100, 0, 0, new Actor().ActorId);
        row.ResolvedMemberId = member.Id;
        row.Status = CheckoffRowStatus.Validated;
        await using (var setup = new CheckOffDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(employer, member, batch, row);
            await setup.SaveChangesAsync();
        }
        var banking = Substitute.For<IBankingUnitOfWork>();
        var product = SavingsProduct.Create("Savings", "SAV", 0, 0, true, 0, null, null, true, new Actor().ActorId);
        banking.SavingsProducts.ListAsync(Arg.Any<Func<IQueryable<SavingsProduct>, IQueryable<SavingsProduct>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SavingsProduct> { product });
        var payments = new CheckoffPaymentSender();
        for (var attempt = 0; attempt < 3; attempt++)
        {
            await using var db = new CheckOffDBContext(options, tenant, new Actor());
            await using var services = Services(db, tenant);
            var uow = services.GetRequiredService<ICheckOffUnitOfWork>();
            var child = new ChildCheckoffChunkJob(uow, banking, Substitute.For<ILoansUnitOfWork>(), payments, tenant);
            var master = new MasterCheckoffBatchJob(uow, child, tenant);
            if (attempt == 0)
                await Assert.ThrowsAsync<InvalidOperationException>(() => master.ExecuteAsync(batch.Id, CancellationToken.None));
            else await master.ExecuteAsync(batch.Id, CancellationToken.None);
            await using var fresh = new CheckOffDBContext(options, tenant, new Actor());
            Assert.Equal(attempt == 0 ? CheckoffRowStatus.Validated : CheckoffRowStatus.Processed,
                (await fresh.CheckoffStagingRows.SingleAsync()).Status);
            Assert.Equal(attempt == 0 ? CheckoffBatchStatus.Posting : CheckoffBatchStatus.Posted,
                (await fresh.CheckoffBatches.SingleAsync()).Status);
            payments.Reject = false;
        }
        Assert.Equal(2, payments.References.Count);
        Assert.All(payments.References, reference => Assert.Equal($"CHK:{row.Id:N}:S", reference));
    }
    [Fact]
    public async Task Journal_replay_preserves_one_posting_and_rejects_changed_entries()
    {
        var tenant = new Tenant();
        var options = Options<AccountingDBContext>();
        var debit = Account.Create(tenant.TenantId, "T-DEBIT", "Test debit", AccountType.Asset, null, new Actor().ActorId);
        var credit = Account.Create(tenant.TenantId, "T-CREDIT", "Test credit", AccountType.Liability, null, new Actor().ActorId);
        await using (var setup = new AccountingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(debit, credit);
            await setup.SaveChangesAsync();
        }
        var date = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero).AddTicks(7);
        Guid? journalId = null;
        foreach (var amount in new[] { 100m, 100m, 90m })
        {
            await using var db = new AccountingDBContext(options, tenant, new Actor());
            await using var services = Services(db, tenant);
            var ledger = new LedgerService(services.GetRequiredService<IAccountingUnitOfWork>(), tenant);
            Task<Guid> Post() => ledger.PostJournalAsync("replay", "test", date,
                [new CreateJournalEntryDto { AccountId = debit.Id, Debit = amount }, new CreateJournalEntryDto { AccountId = credit.Id, Credit = amount }], new Actor().ActorId);
            if (amount == 90m) await Assert.ThrowsAsync<InvalidOperationException>(Post);
            else
            {
                var id = await Post();
                if (journalId is not null) Assert.Equal(journalId, id);
                journalId = id;
            }
        }
        await using var verify = new AccountingDBContext(options, tenant, new Actor());
        Assert.Equal(1, await verify.Journals.CountAsync(j => j.ReferenceNumber == "replay"));
    }

    [Fact]
    public async Task Fosa_rejects_invalid_cash_and_preserves_balances_across_competing_updates()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var account = LS.Domain.Features.Banking.FOSA.Entities.FosaAccount.Create(Guid.CreateVersion7(), "FOSA-TEST", new Actor().ActorId);
        account.Balance = 1000;
        var till = LS.Domain.Features.Banking.FOSA.Entities.TellerTill.Create("Test till", 10000, new Actor().ActorId);
        till.Status = LS.Domain.Features.Banking.FOSA.Enums.TillStatus.Open;
        till.CurrentBalance = 1000;
        var vault = LS.Domain.Features.Banking.FOSA.Entities.Vault.Create("Test vault", new Actor().ActorId);
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(account, till, vault);
            await setup.SaveChangesAsync();
        }
        await using var stale = new BankingDBContext(options, tenant, new Actor());
        var staleAccount = await stale.FosaAccounts.SingleAsync(a => a.Id == account.Id);
        var staleTill = await stale.TellerTills.SingleAsync(t => t.Id == till.Id);
        await using (var db = new BankingDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(db, tenant);
            var sender = services.GetRequiredService<ISender>();
            foreach (var amount in new[] { -10m, 0m, 0.001m })
                Assert.False((await sender.Send(new LS.Application.Features.Banking.FOSA.Commands.OverTheCounterTransactionCommand(
                    till.Id, account.Id, LS.Domain.Features.Banking.FOSA.Enums.OtcTransactionType.CashWithdrawal, amount, "invalid"))).IsSuccess);
            Assert.False((await sender.Send(new LS.Application.Features.Banking.FOSA.Commands.OverTheCounterTransactionCommand(
                till.Id, account.Id, (LS.Domain.Features.Banking.FOSA.Enums.OtcTransactionType)99, 10, "invalid"))).IsSuccess);
            Assert.False((await sender.Send(new LS.Application.Features.Banking.FOSA.Commands.VaultTransferCommand(till.Id, vault.Id,
                new LS.Domain.Features.Banking.FOSA.ValueObjects.DenominationBreakdown(1, -1, 0, 0, 0, 0, 0, 0, 0), "invalid"))).IsSuccess);
            await services.GetRequiredService<IBankingUnitOfWork>().CompleteAsync();
        }
        await using (var verifyRejected = new BankingDBContext(options, tenant, new Actor()))
        {
            Assert.Equal(1000, (await verifyRejected.FosaAccounts.SingleAsync(a => a.Id == account.Id)).Balance);
            Assert.Equal(1000, (await verifyRejected.TellerTills.SingleAsync(t => t.Id == till.Id)).CurrentBalance);
            Assert.Empty(await verifyRejected.FosaTransactions.ToListAsync());
        }
        await using (var db = new BankingDBContext(options, tenant, new Actor()))
        {
            await using var services = Services(db, tenant);
            var sender = services.GetRequiredService<ISender>();
            Assert.True((await sender.Send(new LS.Application.Features.Banking.FOSA.Commands.OverTheCounterTransactionCommand(
                till.Id, account.Id, LS.Domain.Features.Banking.FOSA.Enums.OtcTransactionType.CashWithdrawal, 100, "withdrawal"))).IsSuccess);
            db.ChangeTracker.Clear();
            Assert.True((await sender.Send(new LS.Application.Features.Banking.FOSA.Commands.VaultTransferCommand(till.Id, vault.Id,
                new LS.Domain.Features.Banking.FOSA.ValueObjects.DenominationBreakdown(0, 0, 1, 0, 0, 0, 0, 0, 0), "vault transfer"))).IsSuccess);
        }
        staleAccount.Balance -= 500;
        staleTill.CurrentBalance -= 500;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => stale.SaveChangesAsync());
        await using var verify = new BankingDBContext(options, tenant, new Actor());
        Assert.Equal(900, (await verify.FosaAccounts.SingleAsync(a => a.Id == account.Id)).Balance);
        Assert.Equal(700, (await verify.TellerTills.SingleAsync(t => t.Id == till.Id)).CurrentBalance);
        Assert.Equal(200, (await verify.Vaults.SingleAsync(v => v.Id == vault.Id)).CurrentBalance);
        var transaction = await verify.FosaTransactions.SingleAsync();
        Assert.Equal(100, transaction.Amount);
        Assert.Equal(new Actor().ActorId, transaction.CreatedBy);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Accounting_index_upgrade_restores_uniqueness_or_preserves_conflicting_data(bool existingDuplicates)
    {
        var tenant = new Tenant();
        AccountingDBContext CreateContext() => fixture is PostgreSqlDbFixture
            ? new AccountingPostgreSqlDBContext(new DbContextOptionsBuilder<AccountingPostgreSqlDBContext>(Options<AccountingPostgreSqlDBContext>())
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>().Options, tenant, new Actor())
            : new AccountingSqlServerDBContext(new DbContextOptionsBuilder<AccountingSqlServerDBContext>(Options<AccountingSqlServerDBContext>())
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentSqlServerMigrationsSqlGenerator>().Options, tenant, new Actor());
        await using var db = CreateContext();
        var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        var previous = fixture is PostgreSqlDbFixture ? "20260909221215_AddAccountingDLQ" : "20260909221124_AddAccountingDLQ";
        await migrator.MigrateAsync(previous);
        var indexes = new[] { ("Accounts", "AccountCode"), ("Journals", "ReferenceNumber"), ("TransactionTypeGlMappings", "TransactionTypeCode") };
        var generator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator>();
        foreach (var (table, column) in indexes)
        {
            var name = $"IX_{table}_TenantId_{column}";
            // Reproduce the actual legacy defect: the expected name exists, but UNIQUE is missing.
            Microsoft.EntityFrameworkCore.Migrations.Operations.MigrationOperation[] operations =
            [
                new Microsoft.EntityFrameworkCore.Migrations.Operations.DropIndexOperation { Name = name, Schema = "accounting", Table = table },
                new Microsoft.EntityFrameworkCore.Migrations.Operations.CreateIndexOperation { Name = name, Schema = "accounting", Table = table, Columns = ["TenantId", column], IsUnique = false }
            ];
            foreach (var sql in generator.Generate(operations))
                await db.Database.ExecuteSqlRawAsync(sql.CommandText);
        }
        db.Journals.Add(Journal.Create(tenant.TenantId, "upgrade-reference", "Preserve this journal", DateTimeOffset.UtcNow, new Actor().ActorId));
        if (existingDuplicates)
            db.Journals.Add(Journal.Create(tenant.TenantId, "upgrade-reference", "Preserve this conflict too", DateTimeOffset.UtcNow, new Actor().ActorId));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        if (existingDuplicates)
        {
            await Assert.ThrowsAnyAsync<Exception>(() => migrator.MigrateAsync());
            Assert.DoesNotContain(await db.Database.GetAppliedMigrationsAsync(), m => m.EndsWith("_RepairAccountingUniqueIndexes", StringComparison.Ordinal));
            Assert.Equal(2, await db.Journals.CountAsync(j => j.ReferenceNumber == "upgrade-reference"));
        }
        else
        {
            await migrator.MigrateAsync();
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal(1, await db.Journals.CountAsync(j => j.ReferenceNumber == "upgrade-reference"));
            db.Journals.Add(Journal.Create(tenant.TenantId, "upgrade-reference", "Must be rejected", DateTimeOffset.UtcNow, new Actor().ActorId));
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
        await using var verify = CreateContext();
        var uniqueCount = fixture is PostgreSqlDbFixture
            ? await verify.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM pg_indexes WHERE schemaname = 'accounting' AND indexname IN ('IX_Accounts_TenantId_AccountCode', 'IX_Journals_TenantId_ReferenceNumber', 'IX_TransactionTypeGlMappings_TenantId_TransactionTypeCode') AND indexdef LIKE 'CREATE UNIQUE INDEX%'").SingleAsync()
            : await verify.Database.SqlQueryRaw<int>("SELECT count(*) AS [Value] FROM sys.indexes WHERE is_unique = 1 AND OBJECT_SCHEMA_NAME(object_id) = 'accounting' AND name IN ('IX_Accounts_TenantId_AccountCode', 'IX_Journals_TenantId_ReferenceNumber', 'IX_TransactionTypeGlMappings_TenantId_TransactionTypeCode')").SingleAsync();
        Assert.Equal(existingDuplicates ? 0 : 3, uniqueCount);
    }
    [Fact]
    public async Task Loans_migration_chain_matches_the_current_runtime_model()
    {
        if (fixture is PostgreSqlDbFixture)
        {
            var options = new DbContextOptionsBuilder<LoansPostgreSqlDBContext>(Options<LoansPostgreSqlDBContext>())
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>().Options;
            await using var migrated = new LoansPostgreSqlDBContext(options);
            await migrated.Database.MigrateAsync();
            Assert.False(migrated.Database.HasPendingModelChanges());
        }
        else
        {
            var options = new DbContextOptionsBuilder<LoansSqlServerDBContext>(Options<LoansSqlServerDBContext>())
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentSqlServerMigrationsSqlGenerator>().Options;
            await using var migrated = new LoansSqlServerDBContext(options);
            await migrated.Database.MigrateAsync();
            Assert.False(migrated.Database.HasPendingModelChanges());
        }
        await using var runtime = new LoansDBContext(Options<LoansDBContext>(), new Tenant(), new Actor());
        Assert.Empty(await runtime.LoanApplications.ToListAsync());
        Assert.Empty(await runtime.CollectionCases.ToListAsync());
        Assert.Empty(await runtime.CollectionActions.ToListAsync());
        Assert.Empty(await runtime.CollectionPromises.ToListAsync());
    }
    [Fact]
    public async Task Loan_repayment_replay_saves_one_payment_and_rejects_conflicting_receipt()
    {
        var tenant = new Tenant();
        var options = Options<LoansDBContext>();
        var product = LoanProduct.Create(tenant.TenantId, "TEST", "Test loan", null, 0, InterestMethod.Flat, 12, 10000, new Actor().ActorId);
        var loan = LoanApplication.Create(tenant.TenantId, "TEST-1", Guid.CreateVersion7(), product.Id, 100, 12, 0, InterestMethod.Flat, new Actor().ActorId);
        loan.Status = LoanStatus.Disbursed;
        await using (var setup = new LoansDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Add(product); setup.Add(loan);
            await setup.SaveChangesAsync();
        }
        Guid? paymentId = null;
        foreach (var amount in new[] { 100m, 100m, 90m })
        {
            await using var db = new LoansDBContext(options, tenant, new Actor());
            await using var services = Services(db, tenant);
            var result = await services.GetRequiredService<ISender>().Send(new ProcessRepaymentCommand
                { LoanApplicationId = loan.Id, Amount = amount, ReceiptNumber = "CHK:replay" });
            if (amount == 90m) Assert.False(result.IsSuccess);
            else
            {
                Assert.True(result.IsSuccess, result.Message);
                if (paymentId is not null) Assert.Equal(paymentId, result.Data);
                paymentId = result.Data;
            }
        }
        await using var verify = new LoansDBContext(options, tenant, new Actor());
        Assert.Equal(0, (await verify.Set<LoanApplication>().SingleAsync(l => l.Id == loan.Id)).OutstandingPrincipal);
        Assert.Equal(1, await verify.Set<LoanRepayment>().CountAsync());
    }

    [Fact]
    public async Task Savings_and_share_replay_do_not_credit_twice_or_accept_changed_amounts()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var savings = SavingsProduct.Create("Savings", "SAV", 0, 0, true, 0, null, null, true, new Actor().ActorId);
        var shares = LS.Domain.Features.Banking.Shares.Entities.ShareProduct.Create("Shares", "SHR", 10, 1, new Actor().ActorId);
        var member = Guid.CreateVersion7();
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Add(savings); setup.Add(shares);
            await setup.SaveChangesAsync();
        }
        foreach (var amount in new[] { 100m, 100m, 90m })
        {
            await using var db = new BankingDBContext(options, tenant, new Actor());
            await using var services = Services(db, tenant);
            var sender = services.GetRequiredService<ISender>();
            var deposit = await sender.Send(new DepositSavingsCommand(new DepositSavingsRequest(member, savings.Id, amount, null, "CHK:savings")));
            var purchase = await sender.Send(new LS.Application.Features.Banking.Shares.Commands.PurchaseSharesCommand(member, shares.Id, amount, null, "CHK:shares"));
            Assert.Equal(amount == 100m, deposit.IsSuccess);
            Assert.Equal(amount == 100m, purchase.IsSuccess);
        }
        await using var verify = new BankingDBContext(options, tenant, new Actor());
        Assert.Equal(100m, (await verify.Set<SavingsAccount>().SingleAsync()).Balance);
        Assert.Equal(1, await verify.Set<SavingsTransaction>().CountAsync());
        Assert.Equal(1, await verify.Set<LS.Domain.Features.Banking.Shares.Entities.ShareTransaction>().CountAsync());
    }
    [Fact]
    public async Task Banking_outbox_rolls_back_and_recovers_committed_messages_after_host_restart()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
            await setup.Database.EnsureCreatedAsync();
        var received = new System.Collections.Concurrent.ConcurrentDictionary<Guid, bool>();
        IHost CreateHost()
        {
            LogContext.ConfigureCurrentLogContext(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddSingleton<ICurrentTenantProvider>(tenant);
            builder.Services.AddSingleton<ICurrentActorProvider>(new Actor());
            builder.Services.AddScoped(_ => new BankingDBContext(options, tenant, new Actor()));
            builder.Services.AddMassTransit(bus =>
            {
                bus.AddEntityFrameworkOutbox<BankingDBContext>(outbox =>
                {
                    if (fixture is PostgreSqlDbFixture) outbox.UsePostgres(false); else outbox.UseSqlServer(false);
                    outbox.UseBusOutbox();
                    outbox.QueryDelay = TimeSpan.FromMilliseconds(100);
                });
                bus.UsingInMemory((context, cfg) => cfg.ReceiveEndpoint("phase2-recovery", endpoint =>
                    endpoint.Handler<SavingsDepositedIntegrationEvent>(message =>
                    {
                        received.TryAdd(message.Message.TransactionId, true);
                        return Task.CompletedTask;
                    })));
            });
            return builder.Build();
        }
        SavingsDepositedIntegrationEvent Message(Guid id) => new(Guid.CreateVersion7(), Guid.CreateVersion7(),
            Guid.CreateVersion7(), 100m, "outbox-test") { TenantId = tenant.TenantId, TransactionId = id, OccurredAt = DateTimeOffset.UtcNow };
        var rejectedId = Guid.CreateVersion7();
        var committedIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        using (var writer = CreateHost())
        {
            await using (var scope = writer.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BankingDBContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IScopedBusOutbox<BankingDBContext>>();
                await using var transaction = await db.Database.BeginTransactionAsync();
                await publisher.Publish(Message(rejectedId));
                await db.SaveChangesAsync();
                await transaction.RollbackAsync();
            }
            await using (var fresh = new BankingDBContext(options, tenant, new Actor()))
                Assert.Equal(0, await fresh.Set<OutboxMessage>().CountAsync());
            // Two commits in one scope exercise the same lifecycle as an interest job's successive batches.
            await using (var scope = writer.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BankingDBContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IScopedBusOutbox<BankingDBContext>>();
                foreach (var id in committedIds)
                {
                    await publisher.Publish(Message(id));
                    await db.SaveChangesAsync();
                }
            }
            await using var verify = new BankingDBContext(options, tenant, new Actor());
            Assert.Equal(2, await verify.Set<OutboxMessage>().CountAsync());
            Assert.Empty(received);
        }
        using var recovery = CreateHost();
        await recovery.StartAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            while (committedIds.Any(id => !received.ContainsKey(id)))
                await Task.Delay(100, timeout.Token);
            Assert.False(received.ContainsKey(rejectedId));
        }
        finally { await recovery.StopAsync(); }
    }
    [Fact]
    public async Task Provider_specific_checkoff_context_resolves_tenant_and_actor_through_DI()
    {
        ServiceProvider CreateProvider(Tenant tenant)
        {
            var services = new ServiceCollection().AddLogging();
            services.AddSingleton<ICurrentTenantProvider>(tenant);
            services.AddSingleton<ICurrentActorProvider>(new Actor());
            if (fixture is PostgreSqlDbFixture)
            {
                services.AddSingleton(Options<CheckOffPostgreSqlDBContext>());
                services.AddScoped<CheckOffDBContext, CheckOffPostgreSqlDBContext>();
            }
            else
            {
                services.AddSingleton(Options<CheckOffSqlServerDBContext>());
                services.AddScoped<CheckOffDBContext, CheckOffSqlServerDBContext>();
            }
            return services.BuildServiceProvider();
        }
        var tenant = new Tenant();
        await using (var services = CreateProvider(tenant))
        {
            var context = services.GetRequiredService<CheckOffDBContext>();
            Assert.Equal(tenant.TenantId, context.CurrentTenantId);
            await context.Database.EnsureCreatedAsync();
            var employer = Employer.Create(Guid.Empty, "Employer", "Contact", "contact@example.test", "+254700000001", "");
            context.Add(employer);
            await context.SaveChangesAsync();
            Assert.Equal(tenant.TenantId, employer.TenantId);
            Assert.Equal(new Actor().ActorId, employer.CreatedBy);
        }
        await using (var services = CreateProvider(tenant))
        {
            Assert.Single(await services.GetRequiredService<CheckOffDBContext>().Employers.ToListAsync());
        }
        await using var otherTenant = CreateProvider(new Tenant());
        Assert.Empty(await otherTenant.GetRequiredService<CheckOffDBContext>().Employers.ToListAsync());
    }

    [Fact]
    public async Task Banking_migration_chain_matches_the_current_runtime_model()
    {
        if (fixture is PostgreSqlDbFixture)
        {
            var options = new DbContextOptionsBuilder<BankingPostgreSqlDBContext>(Options<BankingPostgreSqlDBContext>())
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>().Options;
            await using var migrated = new BankingPostgreSqlDBContext(options);
            await migrated.Database.MigrateAsync();
            Assert.False(migrated.Database.HasPendingModelChanges());
        }
        else
        {
            var options = new DbContextOptionsBuilder<BankingSqlServerDBContext>(Options<BankingSqlServerDBContext>())
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentSqlServerMigrationsSqlGenerator>().Options;
            await using var migrated = new BankingSqlServerDBContext(options);
            await migrated.Database.MigrateAsync();
            Assert.False(migrated.Database.HasPendingModelChanges());
        }
        var tenant = new Tenant();
        await using var runtime = new BankingDBContext(Options<BankingDBContext>(), tenant, new Actor());
        // Query the actual tables/columns from the deployed model, including previously missing mappings.
        Assert.Empty(await runtime.SavingsAccounts.ToListAsync());
        Assert.Empty(await runtime.DepositAccounts.ToListAsync());
        Assert.Empty(await runtime.SavingsProducts.ToListAsync());
        Assert.Empty(await runtime.FosaAccounts.ToListAsync());
        Assert.Empty(await runtime.FosaTransactions.ToListAsync());
        Assert.Empty(await runtime.TellerTills.ToListAsync());
        Assert.Empty(await runtime.TillBalancingRecords.ToListAsync());
        Assert.Empty(await runtime.Vaults.ToListAsync());
    }

    [Fact]
    public async Task Interest_jobs_are_repeat_safe_bounded_and_stop_deposit_accrual_at_maturity()
    {
        var tenant = new Tenant();
        var reads = new Reads();
        var options = Options<BankingDBContext>(reads);
        var today = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var savingsProduct = SavingsProduct.Create("Savings", "SAV", 36.5m, 0, true, 0, null, null, true, new Actor().ActorId);
        var depositProduct = new DepositProduct { Name = "Fixed", Code = "FIX", CreatedBy = new Actor().ActorId, InterestRate = 36.5m };
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(savingsProduct, depositProduct);
            for (var i = 0; i < 201; i++)
            {
                var memberId = Guid.CreateVersion7();
                var savings = SavingsAccount.Create(memberId, savingsProduct.Id, new Actor().ActorId);
                savings.Balance = 10000;
                var deposit = DepositAccount.Create(memberId, depositProduct.Id, today.AddDays(2), new Actor().ActorId);
                deposit.Balance = 10000;
                setup.AddRange(savings, deposit);
            }
            await setup.SaveChangesAsync();
        }
        // Repeat each date using a fresh scope/context, as a restarted background worker would.
        foreach (var date in new[] { today, today, today.AddDays(1), today.AddDays(1) })
        {
            await using var context = new BankingDBContext(options, tenant, new Actor());
            await using var services = Services(context, tenant, clock: new FixedClock(date));
            reads.Count = 0;
            await ActivatorUtilities.CreateInstance<CalculateSavingsInterestJob>(services).ExecuteAsync(CancellationToken.None);
            await ActivatorUtilities.CreateInstance<CalculateDepositInterestJob>(services).ProcessAsync(CancellationToken.None);
            Assert.InRange(reads.Count, 1, 8);
        }
        await using (var fresh = new BankingDBContext(options, tenant, new Actor()))
        {
            Assert.All(await fresh.SavingsAccounts.ToListAsync(), a => { Assert.Equal(10020.01m, a.Balance); Assert.Equal(new DateOnly(2030, 1, 2), a.LastInterestAccruedOn); });
            Assert.All(await fresh.DepositAccounts.ToListAsync(), a => Assert.Equal(20m, a.AccruedInterest));
            Assert.Equal(402, await fresh.SavingsTransactions.CountAsync());
        }
        for (var run = 0; run < 2; run++)
        {
            await using var context = new BankingDBContext(options, tenant, new Actor());
            await using var services = Services(context, tenant, clock: new FixedClock(today.AddDays(2)));
            await ActivatorUtilities.CreateInstance<CalculateDepositInterestJob>(services).ProcessAsync(CancellationToken.None);
            await ActivatorUtilities.CreateInstance<ProcessDepositMaturitiesJob>(services).ProcessAsync(CancellationToken.None);
        }
        await using var final = new BankingDBContext(options, tenant, new Actor());
        Assert.All(await final.DepositAccounts.ToListAsync(), a => { Assert.Equal(10020m, a.Balance); Assert.Equal(0m, a.AccruedInterest); });
        Assert.Equal(201, await final.DepositTransactions.CountAsync());
    }

    [Fact]
    public async Task Failed_interest_batch_leaves_no_partial_balances_dates_or_transactions()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var product = SavingsProduct.Create("Savings", "SAV", 36.5m, 0, true, 0, null, null, true, new Actor().ActorId);
        await using var context = new BankingDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        context.Add(product);
        for (var i = 0; i < 2; i++)
        {
            var account = SavingsAccount.Create(Guid.CreateVersion7(), product.Id, new Actor().ActorId);
            account.Balance = 10000;
            context.Add(account);
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        await using var services = Services(context, tenant, failInterestPublication: true);
        await Assert.ThrowsAsync<InvalidOperationException>(() => ActivatorUtilities.CreateInstance<CalculateSavingsInterestJob>(services).ExecuteAsync(CancellationToken.None));
        await services.GetRequiredService<IBankingUnitOfWork>().CompleteAsync();
        await using var fresh = new BankingDBContext(options, tenant, new Actor());
        Assert.All(await fresh.SavingsAccounts.ToListAsync(), a => { Assert.Equal(10000m, a.Balance); Assert.Null(a.LastInterestAccruedOn); });
        Assert.False(await fresh.SavingsTransactions.AnyAsync());
    }

    [Fact]
    public async Task Banking_rejects_competing_detached_balance_updates()
    {
        var tenant = new Tenant();
        var options = Options<BankingDBContext>();
        var product = SavingsProduct.Create("Savings", "SAV", 0, 0, true, 0, null, null, true, new Actor().ActorId);
        var account = SavingsAccount.Create(Guid.CreateVersion7(), product.Id, new Actor().ActorId);
        await using (var setup = new BankingDBContext(options, tenant, new Actor()))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.AddRange(product, account);
            await setup.SaveChangesAsync();
        }
        await using var first = new BankingDBContext(options, tenant, new Actor());
        await using var second = new BankingDBContext(options, tenant, new Actor());
        await using var firstServices = Services(first, tenant);
        await using var secondServices = Services(second, tenant);
        var firstUow = firstServices.GetRequiredService<IBankingUnitOfWork>();
        var secondUow = secondServices.GetRequiredService<IBankingUnitOfWork>();
        var a = await firstUow.SavingsAccounts.FirstOrDefaultAsync(x => x.Id == account.Id);
        var b = await secondUow.SavingsAccounts.FirstOrDefaultAsync(x => x.Id == account.Id);
        a!.Balance += 100;
        b!.Balance += 200;
        await firstUow.SavingsAccounts.UpdateAsync(a);
        await secondUow.SavingsAccounts.UpdateAsync(b);
        await firstUow.CompleteAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondUow.CompleteAsync());
        await using var fresh = new BankingDBContext(options, tenant, new Actor());
        Assert.Equal(100m, (await fresh.SavingsAccounts.SingleAsync()).Balance);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Retry_failure_clears_writes_even_after_database_save(bool concurrencyFailure)
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        await using var context = new HrDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        await using var services = Services(context, tenant);
        var uow = services.GetRequiredService<IHrUnitOfWork>();
        var attempts = 0;
        var original = concurrencyFailure ? new DbUpdateConcurrencyException("Injected conflict") : new InvalidOperationException("Injected failure") as Exception;
        var thrown = await Assert.ThrowsAnyAsync<Exception>(() => uow.ExecuteInTransactionWithRetryAsync<int>(async () =>
        {
            attempts++;
            await uow.PayrollPeriodRepository.CreateAsync(PayrollPeriod.Create(2025, attempts, new Actor().ActorId));
            await context.SaveChangesAsync();
            throw original;
        }, maxRetries: 2, baseDelayMs: 0));
        Assert.Same(original, thrown);
        Assert.Equal(concurrencyFailure ? 2 : 1, attempts);
        Assert.Empty(context.ChangeTracker.Entries());
        await uow.CompleteAsync();
        await using var fresh = new HrDBContext(options, tenant, new Actor());
        Assert.False(await fresh.PayrollPeriods.AnyAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Retry_cancellation_rolls_back_and_stops_further_attempts(bool duringBackoff)
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        await using var context = new HrDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        await using var services = Services(context, tenant);
        var uow = services.GetRequiredService<IHrUnitOfWork>();
        using var cancellation = new CancellationTokenSource();
        var attempts = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => uow.ExecuteInTransactionWithRetryAsync(async () =>
        {
            attempts++;
            await uow.PayrollPeriodRepository.CreateAsync(PayrollPeriod.Create(2025, attempts, new Actor().ActorId));
            await context.SaveChangesAsync();
            cancellation.Cancel();
            if (duringBackoff) throw new DbUpdateConcurrencyException("Injected conflict before backoff");
            return true;
        }, baseDelayMs: 60000, cancellationToken: cancellation.Token));
        Assert.Equal(1, attempts);
        Assert.Empty(context.ChangeTracker.Entries());
        await uow.CompleteAsync();
        await using var fresh = new HrDBContext(options, tenant, new Actor());
        Assert.False(await fresh.PayrollPeriods.AnyAsync());
    }

    [Fact]
    public async Task Retry_reloads_after_conflict_and_commits_only_the_successful_attempt()
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        await using var context = new HrDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        await using var services = Services(context, tenant);
        var uow = services.GetRequiredService<IHrUnitOfWork>();
        var attempts = 0;
        var result = await uow.ExecuteInTransactionWithRetryAsync(async () =>
        {
            attempts++;
            Assert.False(await uow.PayrollPeriodRepository.AnyAsync(p => p.Year == 2025));
            await uow.PayrollPeriodRepository.CreateAsync(PayrollPeriod.Create(2025, attempts, new Actor().ActorId));
            if (attempts == 1)
            {
                await context.SaveChangesAsync();
                throw new DbUpdateConcurrencyException("Injected first-attempt conflict");
            }
            return attempts;
        }, baseDelayMs: 0);
        Assert.Equal(2, result);
        await using var fresh = new HrDBContext(options, tenant, new Actor());
        Assert.Equal(2, (await fresh.PayrollPeriods.SingleAsync()).Month);
    }

    [Fact]
    public async Task Explicit_commit_predicate_rolls_back_already_saved_writes_on_expected_rejection()
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        await using var context = new HrDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        await using var services = Services(context, tenant);
        var uow = services.GetRequiredService<IHrUnitOfWork>();
        var result = await uow.ExecuteInTransactionWithRetryAsync(async () =>
        {
            await uow.PayrollPeriodRepository.CreateAsync(PayrollPeriod.Create(2025, 1, new Actor().ActorId));
            // Reproduce an embedded persistence API (such as Identity) saving before returning failure.
            await context.SaveChangesAsync();
            return AppResponses.Failure<bool>(AppError.BusinessRule("Expected rejection after an embedded save."));
        }, shouldCommit: response => response.IsSuccess);
        Assert.False(result.IsSuccess);
        Assert.Empty(context.ChangeTracker.Entries());
        await uow.CompleteAsync();
        await using var fresh = new HrDBContext(options, tenant, new Actor());
        Assert.Empty(await fresh.PayrollPeriods.ToListAsync());
    }
    [Fact]
    public async Task Rejected_result_can_still_commit_intentional_diagnostics()
    {
        var tenant = new Tenant();
        var options = Options<HrDBContext>();
        await using var context = new HrDBContext(options, tenant, new Actor());
        await context.Database.EnsureCreatedAsync();
        await using var services = Services(context, tenant);
        var uow = services.GetRequiredService<IHrUnitOfWork>();
        var result = await uow.ExecuteInTransactionWithRetryAsync(async () =>
        {
            await uow.PayrollPeriodRepository.CreateAsync(PayrollPeriod.Create(2025, 1, new Actor().ActorId));
            return LS.SharedKernel.Dtos.Common.AppResponses.Failure<bool>("Expected rejection after intentional writes.");
        });
        Assert.False(result.IsSuccess);
        await using var fresh = new HrDBContext(options, tenant, new Actor());
        Assert.Single(await fresh.PayrollPeriods.ToListAsync());
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
