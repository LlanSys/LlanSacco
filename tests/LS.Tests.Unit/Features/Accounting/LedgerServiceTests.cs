using LS.SharedKernel.Features.Accounting.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Accounting.Services;
using LS.Domain.Features.Accounting.Contracts;

using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts.Common;
using NSubstitute;
using Xunit;

namespace LS.Tests.Unit.Features.Accounting;

public class LedgerServiceTests
{
    private readonly IAccountingUnitOfWork _uow;
    private readonly ICurrentTenantProvider _tenantProvider;
    private readonly LedgerService _service;

    public LedgerServiceTests()
    {
        _uow = Substitute.For<IAccountingUnitOfWork>();
        _tenantProvider = Substitute.For<ICurrentTenantProvider>();
        _tenantProvider.TenantId.Returns(Guid.NewGuid());

        _service = new LedgerService(_uow, _tenantProvider);
    }

    [Fact]
    public async Task PostJournalAsync_ValidBalancedEntry_Succeeds()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var entries = new List<CreateJournalEntryDto>
        {
            new CreateJournalEntryDto { AccountId = accountId1, Debit = 100, Credit = 0, Description = "Debit entry" },
            new CreateJournalEntryDto { AccountId = accountId2, Debit = 0, Credit = 100, Description = "Credit entry" }
        };

        // Act
        var journalId = await _service.PostJournalAsync(
            "REF-001",
            "Test Journal",
            DateTimeOffset.UtcNow,
            entries,
            "TestUser",
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, journalId);
        await _uow.JournalRepository.Received(1).CreateAsync(Arg.Any<Journal>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostJournalAsync_UnbalancedEntry_ThrowsInvalidOperationException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var entries = new List<CreateJournalEntryDto>
        {
            new CreateJournalEntryDto { AccountId = accountId1, Debit = 100, Credit = 0, Description = "Debit entry" },
            new CreateJournalEntryDto { AccountId = accountId2, Debit = 0, Credit = 90, Description = "Credit entry" }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PostJournalAsync(
                "REF-001",
                "Test Journal",
                DateTimeOffset.UtcNow,
                entries,
                "TestUser",
                CancellationToken.None));

        Assert.Contains("unbalanced", ex.Message);
    }

    [Fact]
    public async Task PostJournalAsync_NegativeValues_ThrowsInvalidOperationException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var entries = new List<CreateJournalEntryDto>
        {
            new CreateJournalEntryDto { AccountId = accountId1, Debit = -100, Credit = 0, Description = "Debit entry" },
            new CreateJournalEntryDto { AccountId = accountId2, Debit = 0, Credit = -100, Description = "Credit entry" }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PostJournalAsync(
                "REF-001",
                "Test Journal",
                DateTimeOffset.UtcNow,
                entries,
                "TestUser",
                CancellationToken.None));

        Assert.Contains("positive non-zero", ex.Message);
    }

    [Fact]
    public async Task PostJournalAsync_BothDebitAndCreditOnSameLine_ThrowsInvalidOperationException()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var entries = new List<CreateJournalEntryDto>
        {
            new CreateJournalEntryDto { AccountId = accountId1, Debit = 100, Credit = 100, Description = "Both entry" },
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PostJournalAsync(
                "REF-001",
                "Test Journal",
                DateTimeOffset.UtcNow,
                entries,
                "TestUser",
                CancellationToken.None));

        Assert.Contains("cannot have both a debit and a credit", ex.Message);
    }
}
