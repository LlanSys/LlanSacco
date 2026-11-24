using LS.Application.Features.Accounting.Commands;
using LS.Application.Features.Accounting.Validators.UpsertGlMapping;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Features.Shared.Common.Enums;
using NSubstitute;
using System.Linq.Expressions;

namespace LS.Tests.Unit.Features.Accounting;

public class UpsertGlMappingTests
{
    private const string Recovery = "GUARANTOR_RECOVERY";
    private const string Actor = "finance-user";
    private readonly Guid _tenantId = Guid.CreateVersion7();
    private readonly IAccountingUnitOfWork _uow = Substitute.For<IAccountingUnitOfWork>();
    private readonly ICurrentTenantProvider _tenantProvider = Substitute.For<ICurrentTenantProvider>();
    private readonly ICurrentActorProvider _actorProvider = Substitute.For<ICurrentActorProvider>();
    private readonly UpsertGlMappingCommandValidator _validator = new();

    public UpsertGlMappingTests()
    {
        _tenantProvider.TenantId.Returns(_tenantId);
        _actorProvider.ActorId.Returns(Actor);
    }

    [Fact]
    public void Valid_recovery_mapping_passes_validation()
    {
        var result = _validator.Validate(Command(Recovery, Guid.CreateVersion7(), Guid.CreateVersion7()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Same_account_on_both_sides_is_rejected()
    {
        var id = Guid.CreateVersion7();

        Assert.False(_validator.Validate(Command(Recovery, id, id)).IsValid);
    }

    [Fact]
    public void Explicit_gl_without_account_is_rejected() => Assert.False(_validator.Validate(Command(Recovery, null, Guid.CreateVersion7())).IsValid);

    [Theory]
    [InlineData("NONE")]
    [InlineData("explicit_gl")]
    [InlineData("")]
    public void Unknown_source_is_rejected(string source)
    {
        var command = new UpsertGlMappingCommand("MEMBER_DEPOSIT", Request(Guid.CreateVersion7(), Guid.CreateVersion7()) with { FeeSideSource = source });

        Assert.False(_validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData("guarantor_recovery")]
    [InlineData("BAD-CODE")]
    [InlineData("")]
    public void Invalid_code_is_rejected(string code) => Assert.False(_validator.Validate(Command(code, Guid.CreateVersion7(), Guid.CreateVersion7())).IsValid);

    [Fact]
    public void Code_longer_than_64_characters_is_rejected() => Assert.False(_validator.Validate(Command(new string('A', 65), Guid.CreateVersion7(), Guid.CreateVersion7())).IsValid);

    [Fact]
    public void Explicit_id_without_explicit_source_is_rejected()
    {
        var command = new UpsertGlMappingCommand("MEMBER_DEPOSIT", Request(Guid.CreateVersion7(), Guid.CreateVersion7()) with { DebitSideSource = "CHANNEL_ACCOUNT" });

        Assert.False(_validator.Validate(command).IsValid);
    }

    [Fact]
    public void Recovery_mapping_must_use_explicit_accounts()
    {
        var command = new UpsertGlMappingCommand(Recovery, Request(null, Guid.CreateVersion7()) with { DebitSideSource = "CHANNEL_ACCOUNT" });

        Assert.False(_validator.Validate(command).IsValid);
    }

    [Fact]
    public async Task Recovery_mapping_with_liability_debit_and_asset_credit_is_saved()
    {
        var liability = Account(AccountType.Liability);
        var asset = Account(AccountType.Asset);
        Accounts(liability, asset);

        var result = await Handle(Command(Recovery, liability.Id, asset.Id));

        Assert.True(result.IsSuccess);
        await _uow.TransactionTypeGlMappingRepository.Received(1).CreateAsync(
            Arg.Is<TransactionTypeGlMapping>(m => m.TransactionTypeCode == Recovery && m.TenantId == _tenantId && m.CreatedBy == Actor),
            Arg.Any<CancellationToken>());

        await _uow.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Existing_mapping_is_updated_not_duplicated()
    {
        var liability = Account(AccountType.Liability);
        var asset = Account(AccountType.Asset);
        Accounts(liability, asset);
        var existing = new TransactionTypeGlMapping
        {
            TenantId = _tenantId,
            TransactionTypeCode = Recovery,
            DebitSideSource = "CHANNEL_ACCOUNT",
            CreditSideSource = "PRODUCT_ACCOUNT",
            FeeSideSource = "CHANNEL_ACCOUNT",
            CreatedBy = "seed"
        };

        _uow.TransactionTypeGlMappingRepository
            .FirstOrDefaultAsync(Arg.Any<Expression<Func<TransactionTypeGlMapping, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await Handle(Command(Recovery, liability.Id, asset.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Data);
        Assert.Equal(liability.Id, existing.DebitExplicitGlAccountId);
        Assert.Equal(Actor, existing.UpdatedBy);
        await _uow.TransactionTypeGlMappingRepository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
        await _uow.TransactionTypeGlMappingRepository.DidNotReceiveWithAnyArgs().CreateAsync(default!);
    }

    [Fact]
    public async Task Recovery_mapping_with_swapped_liability_and_asset_is_rejected()
    {
        var liability = Account(AccountType.Liability);
        var asset = Account(AccountType.Asset);
        Accounts(liability, asset);

        var result = await Handle(Command(Recovery, asset.Id, liability.Id));

        Assert.Equal(ErrorType.BusinessRule, result.Error?.Type);
        await AssertNothingWritten();
    }

    [Fact]
    public async Task Inactive_explicit_account_is_rejected()
    {
        var liability = Account(AccountType.Liability);
        var asset = Account(AccountType.Asset);
        asset.IsActive = false;
        Accounts(liability, asset);

        var result = await Handle(Command(Recovery, liability.Id, asset.Id));

        Assert.Equal(ErrorType.BusinessRule, result.Error?.Type);
        await AssertNothingWritten();
    }

    [Fact]
    public async Task Unknown_or_other_tenant_account_is_not_found()
    {
        var liability = Account(AccountType.Liability);
        var foreign = LS.Domain.Features.Accounting.Entities.Account.Create(Guid.CreateVersion7(), "1100", "Foreign", AccountType.Asset, null, "seed");
        Accounts(liability, foreign);

        var result = await Handle(Command(Recovery, liability.Id, foreign.Id));

        Assert.Equal(ErrorType.NotFound, result.Error?.Type);
        await AssertNothingWritten();
    }

    private Task<AppResponse<Guid>> Handle(UpsertGlMappingCommand command) =>
        new UpsertGlMappingCommandHandler(_uow, _tenantProvider, _actorProvider).Handle(command, CancellationToken.None);

    private async Task AssertNothingWritten()
    {
        await _uow.TransactionTypeGlMappingRepository.DidNotReceiveWithAnyArgs().CreateAsync(default!);
        await _uow.TransactionTypeGlMappingRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
        await _uow.DidNotReceiveWithAnyArgs().CompleteAsync();
    }

    private void Accounts(params Account[] accounts) =>
        _uow.AccountRepository
            .ListAsync(Arg.Any<Func<IQueryable<Account>, IQueryable<Account>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<IQueryable<Account>, IQueryable<Account>>>()(accounts.AsQueryable()).ToList());

    private Account Account(AccountType type) =>
        LS.Domain.Features.Accounting.Entities.Account.Create(_tenantId, $"{(int)type}000", type.ToString(), type, null, "seed");

    private static UpsertGlMappingCommand Command(string code, Guid? debit, Guid? credit) =>
        new(code, Request(debit, credit));

    private static UpsertGlMappingRequest Request(Guid? debit, Guid? credit) => new()
    {
        DebitSideSource = "EXPLICIT_GL",
        DebitExplicitGlAccountId = debit,
        CreditSideSource = "EXPLICIT_GL",
        CreditExplicitGlAccountId = credit,
        FeeSideSource = "CHANNEL_ACCOUNT",
        Description = "Guarantor recovery"
    };
}
