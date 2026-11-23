using LS.Application.Features.Accounting.Services;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Features.Shared.Common.Enums;
using NSubstitute;
using System.Linq.Expressions;

namespace LS.Tests.Unit.Features.Accounting;

public class LedgerServiceTests
{
    private readonly IAccountingUnitOfWork _uow = Substitute.For<IAccountingUnitOfWork>();
    private readonly ICurrentTenantProvider _tenantProvider = Substitute.For<ICurrentTenantProvider>();

    public LedgerServiceTests() => _tenantProvider.TenantId.Returns(Guid.CreateVersion7());

    [Fact]
    public async Task Balanced_journal_is_saved_once()
    {
        _uow.AccountRepository.CountAsync(Arg.Any<Expression<Func<Account, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(2);
        var service = new LedgerService(_uow, _tenantProvider);
        var result = await service.PostJournalAsync("REF-001", "Test journal", DateTimeOffset.UtcNow,
            [new() { AccountId = Guid.CreateVersion7(), Debit = 100 }, new() { AccountId = Guid.CreateVersion7(), Credit = 100 }],
            ICurrentActorProvider.SystemActor);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);
        await _uow.JournalRepository.Received(1).CreateAsync(Arg.Any<Journal>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("unbalanced", ErrorType.BusinessRule)]
    [InlineData("zero", ErrorType.BusinessRule)]
    [InlineData("negative", ErrorType.Validation)]
    [InlineData("both-sides", ErrorType.Validation)]
    [InlineData("missing-account", ErrorType.NotFound)]
    public async Task Expected_journal_rejection_does_not_stage_or_commit_a_write(string scenario, ErrorType expected)
    {
        var id = Guid.CreateVersion7();
        CreateJournalEntryRequest[] entries = scenario switch
        {
            "unbalanced" => [new() { AccountId = id, Debit = 100 }, new() { AccountId = id, Credit = 90 }],
            "zero" => [],
            "negative" => [new() { AccountId = id, Debit = -100 }, new() { AccountId = id, Credit = -100 }],
            "both-sides" => [new() { AccountId = id, Debit = 100, Credit = 100 }],
            _ => [new() { AccountId = id, Debit = 100 }, new() { AccountId = Guid.CreateVersion7(), Credit = 100 }]
        };
        var service = new LedgerService(_uow, _tenantProvider);
        var result = await service.PostJournalAsync("REF-001", "Test journal", DateTimeOffset.UtcNow,
            entries, ICurrentActorProvider.SystemActor);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected, result.Error?.Type);
        if (expected == ErrorType.Validation)
            Assert.Contains(nameof(CreateJournalRequest.Entries), result.Error!.ValidationErrors!);
        await _uow.JournalRepository.DidNotReceiveWithAnyArgs().CreateAsync(default!);
        await _uow.DidNotReceiveWithAnyArgs().CompleteAsync();
    }
}
