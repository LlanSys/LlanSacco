using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Contracts.Repositories;


namespace LS.Domain.Features.Membership.Contracts;

public interface IMembershipUnitOfWork : ITransactionalUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    IMemberAccountRepository MemberAccountRepository { get; }
    IRepository<MemberTransaction> MemberTransactions { get; }
    IRepository<MemberGuarantor> MemberGuarantors { get; }
    IRepository<GuarantorRequest> GuarantorRequests { get; }
    IRepository<Beneficiary> Beneficiaries { get; }
}
