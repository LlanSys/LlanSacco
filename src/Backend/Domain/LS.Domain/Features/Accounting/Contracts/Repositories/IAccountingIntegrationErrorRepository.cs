using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Domain.Features.Accounting.Contracts.Repositories;

public interface IAccountingIntegrationErrorRepository : IRepository<AccountingIntegrationError>
{
}
