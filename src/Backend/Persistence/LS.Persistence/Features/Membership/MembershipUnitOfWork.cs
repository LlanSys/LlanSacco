using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Contracts.Repositories;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.Membership.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Membership;

public class MembershipUnitOfWork(
    MembershipDBContext context,
    IMemberRepository memberRepository,
    IMemberAccountRepository memberAccountRepository,
    IMemberTransactionRepository memberTransactionRepository,
    IMemberGuarantorRepository memberGuarantorRepository,
    IGuarantorRequestRepository guarantorRequestRepository,
    IBeneficiaryRepository beneficiaryRepository,
    IPublisher publisher,
    ILogger<MembershipUnitOfWork> logger
) : BaseUnitOfWork<MembershipDBContext>(context, publisher, logger), IMembershipUnitOfWork
{
    public IMemberRepository MemberRepository { get; } = memberRepository;
    public IMemberAccountRepository MemberAccountRepository { get; } = memberAccountRepository;
    public IRepository<MemberTransaction> MemberTransactions { get; } = memberTransactionRepository;
    public IRepository<MemberGuarantor> MemberGuarantors { get; } = memberGuarantorRepository;
    public IRepository<GuarantorRequest> GuarantorRequests { get; } = guarantorRequestRepository;
    public IRepository<Beneficiary> Beneficiaries { get; } = beneficiaryRepository;
}
