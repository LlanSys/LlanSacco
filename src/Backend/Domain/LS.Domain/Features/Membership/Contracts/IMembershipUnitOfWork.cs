using LS.Domain.Shared.Contracts;
using LS.Domain.Features.Membership.Contracts.Repositories;


namespace LS.Domain.Features.Membership.Contracts;

public interface IMembershipUnitOfWork : ITransactionalUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    IMemberAccountRepository MemberAccountRepository { get; }
    IMemberTransactionRepository MemberTransactionRepository { get; }
    IMemberGuarantorRepository MemberGuarantorRepository { get; }
    IGuarantorRequestRepository GuarantorRequestRepository { get; }
    IBeneficiaryRepository BeneficiaryRepository { get; }
}
