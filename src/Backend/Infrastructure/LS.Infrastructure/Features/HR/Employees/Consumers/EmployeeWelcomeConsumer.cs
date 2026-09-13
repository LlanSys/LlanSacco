using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Application.Features.HR.Employees.IntegrationEvents;
using LS.Application.Features.IAM.Users.IntegrationEvents;
using LS.Domain.Shared.Contracts;
using LS.Infrastructure.Messaging.Consumers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Infrastructure.Features.HR.Employees.Consumers;


internal sealed class EmployeeWelcomeConsumer(
    IEmailComposer<EmployeeCreatedIntegrationEvent> composer,
    ISharedUnitOfWork shared,
    ILogger<IntegrationEventEmailConsumer<EmployeeCreatedIntegrationEvent>> logger)
    : IntegrationEventEmailConsumer<EmployeeCreatedIntegrationEvent>(composer, shared, logger)
{ 

}
