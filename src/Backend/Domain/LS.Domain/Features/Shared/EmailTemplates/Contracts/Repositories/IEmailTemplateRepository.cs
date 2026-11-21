using LS.Domain.Features.Shared.EmailTemplates.Entities;
using LS.Domain.Shared.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.Shared.EmailTemplates.Contracts.Repositories;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
}
